#!/usr/bin/env python3
"""
Code indexer for RAG system.

Recursively indexes code files from a directory, chunks them intelligently
at class/method boundaries, and stores them in the vector database.
"""

import argparse
import sys
from pathlib import Path
from typing import Dict, List, Set

from vector_store import VectorStore


class CodeIndexer:
    """Intelligent code indexer with syntax-aware chunking."""

    # File extensions to index
    SUPPORTED_EXTENSIONS = {".cs", ".md", ".txt"}

    # Directories to skip
    SKIP_DIRS = {
        "bin",
        "obj",
        "node_modules",
        ".git",
        ".vs",
        ".vscode",
        ".idea",
        "packages",
        "__pycache__",
        ".godot",
        ".import",
        ".mono",
    }

    # Minimum chunk requirements
    MIN_CHUNK_CHARS = 50
    MIN_CHUNK_LINES = 5

    def __init__(self, root_path: str):
        """
        Initialize the indexer.

        Args:
            root_path: Root directory to index

        Raises:
            ValueError: If root_path doesn't exist
        """
        self.root_path = Path(root_path).resolve()
        if not self.root_path.exists():
            raise ValueError(f"Path does not exist: {root_path}")

        if not self.root_path.is_dir():
            raise ValueError(f"Path is not a directory: {root_path}")

        self.files_processed = 0
        self.files_skipped = 0
        self.total_chunks = 0

    def should_skip_directory(self, dir_path: Path) -> bool:
        """
        Check if directory should be skipped.

        Args:
            dir_path: Directory path to check

        Returns:
            True if directory should be skipped
        """
        return dir_path.name in self.SKIP_DIRS

    def is_supported_file(self, file_path: Path) -> bool:
        """
        Check if file should be indexed.

        Args:
            file_path: File path to check

        Returns:
            True if file should be indexed
        """
        return file_path.suffix.lower() in self.SUPPORTED_EXTENSIONS

    def find_files(self) -> List[Path]:
        """
        Recursively find all supported files.

        Returns:
            List of file paths to index
        """
        files = []

        for item in self.root_path.rglob("*"):
            # Skip directories in skip list
            if any(self.should_skip_directory(parent) for parent in item.parents):
                continue

            if item.is_file() and self.is_supported_file(item):
                files.append(item)

        return sorted(files)

    def chunk_code(
        self,
        content: str,
        file_path: Path,
    ) -> List[Dict]:
        """
        Chunk code at class/method boundaries based on brace depth.

        Args:
            content: File content
            file_path: Source file path

        Returns:
            List of chunk dictionaries
        """
        chunks = []
        lines = content.split("\n")

        # For non-code files, use simple chunking
        if file_path.suffix.lower() in {".md", ".txt"}:
            return self._chunk_text(content, file_path)

        # Track brace depth for C# code
        current_chunk = []
        chunk_start_line = 1
        brace_depth = 0
        line_number = 1

        for line in lines:
            current_chunk.append(line)

            # Count braces
            for char in line:
                if char == "{":
                    brace_depth += 1
                elif char == "}":
                    brace_depth -= 1

            # When brace depth returns to 0 and we have content, create chunk
            if brace_depth == 0 and current_chunk:
                chunk_content = "\n".join(current_chunk).strip()

                # Only create chunk if it meets minimum requirements
                if (
                    len(chunk_content) >= self.MIN_CHUNK_CHARS
                    and len(current_chunk) >= self.MIN_CHUNK_LINES
                ):
                    chunks.append(
                        {
                            "content": chunk_content,
                            "source": file_path.name,
                            "file_path": str(file_path),
                            "start_line": chunk_start_line,
                            "end_line": line_number,
                        }
                    )

                current_chunk = []
                chunk_start_line = line_number + 1

            line_number += 1

        # Add remaining content as final chunk if significant
        if current_chunk:
            chunk_content = "\n".join(current_chunk).strip()
            if (
                len(chunk_content) >= self.MIN_CHUNK_CHARS
                and len(current_chunk) >= self.MIN_CHUNK_LINES
            ):
                chunks.append(
                    {
                        "content": chunk_content,
                        "source": file_path.name,
                        "file_path": str(file_path),
                        "start_line": chunk_start_line,
                        "end_line": line_number,
                    }
                )

        return chunks

    def _chunk_text(self, content: str, file_path: Path) -> List[Dict]:
        """
        Chunk text files by paragraphs/sections.

        Args:
            content: File content
            file_path: Source file path

        Returns:
            List of chunk dictionaries
        """
        chunks = []
        paragraphs = content.split("\n\n")
        line_number = 1

        for para in paragraphs:
            para = para.strip()
            if len(para) >= self.MIN_CHUNK_CHARS:
                num_lines = para.count("\n") + 1
                chunks.append(
                    {
                        "content": para,
                        "source": file_path.name,
                        "file_path": str(file_path),
                        "start_line": line_number,
                        "end_line": line_number + num_lines - 1,
                    }
                )
                line_number += num_lines
            else:
                line_number += para.count("\n") + 1

            # Account for blank line separator
            line_number += 2

        return chunks

    def process_file(self, file_path: Path) -> List[Dict]:
        """
        Read and chunk a single file.

        Args:
            file_path: File to process

        Returns:
            List of chunks from the file
        """
        try:
            # Read file with UTF-8 encoding
            content = file_path.read_text(encoding="utf-8", errors="ignore")

            # Skip empty files
            if not content.strip():
                return []

            # Chunk the content
            chunks = self.chunk_code(content, file_path)

            return chunks

        except Exception as e:
            print(f"  ⚠️  Error processing {file_path.name}: {e}")
            self.files_skipped += 1
            return []

    def index(
        self,
        vector_store: VectorStore,
        remove_deleted: bool = True,
    ) -> None:
        """
        Index all files in the root directory.

        Args:
            vector_store: VectorStore instance to add chunks to
            remove_deleted: If True, removes chunks from files that no longer exist
        """
        print(f"\n{'='*60}")
        print(f"Indexing directory: {self.root_path}")
        print(f"{'='*60}\n")

        # Find all files to index
        print("Finding files...")
        files = self.find_files()
        total_files = len(files)

        if total_files == 0:
            print("❌ No supported files found!")
            print(f"   Supported extensions: {', '.join(self.SUPPORTED_EXTENSIONS)}")
            return

        print(f"Found {total_files} files to index\n")

        # Detect and remove chunks from deleted files
        if remove_deleted:
            self._remove_deleted_files(vector_store, files)

        # Process each file
        all_chunks = []
        for i, file_path in enumerate(files, 1):
            rel_path = file_path.relative_to(self.root_path)
            print(f"[{i}/{total_files}] Processing: {rel_path}")

            chunks = self.process_file(file_path)

            if chunks:
                all_chunks.extend(chunks)
                print(f"  + Created {len(chunks)} chunks")
                self.files_processed += 1
            else:
                print(f"  - Skipped (no chunks)")
                self.files_skipped += 1

        # Add all chunks to vector store (smart update by default)
        if all_chunks:
            print(f"\n{'='*60}")
            print("Adding chunks to vector database...")
            print(f"{'='*60}\n")

            added = vector_store.add_chunks(all_chunks, update_existing=True)
            self.total_chunks = added

        # Print summary
        self._print_summary()

    def _remove_deleted_files(
        self,
        vector_store: VectorStore,
        current_files: List[Path],
    ) -> None:
        """
        Remove chunks from files that no longer exist.

        Args:
            vector_store: VectorStore instance
            current_files: List of current file paths
        """
        # Get all file paths currently in the database
        try:
            stats = vector_store.get_stats()
            if stats.get("total_chunks", 0) == 0:
                return  # Nothing to clean up

            # Get list of files in database that are within our root path
            df = vector_store.table.to_pandas()
            db_files = set(df["file_path"].unique())

            # Convert current files to absolute paths as strings
            current_file_paths = set(str(f.resolve()) for f in current_files)

            # Find files that are in database but not on disk
            # Only consider files under our root_path
            root_str = str(self.root_path.resolve())
            deleted_files = [
                fp
                for fp in db_files
                if fp.startswith(root_str) and fp not in current_file_paths
            ]

            if deleted_files:
                print(f"\n{'='*60}")
                print(f"Cleaning up {len(deleted_files)} deleted files...")
                print(f"{'='*60}\n")

                for file_path in deleted_files:
                    rel_path = Path(file_path).relative_to(self.root_path)
                    print(f"  Removing chunks from deleted file: {rel_path}")
                    vector_store.remove_chunks_by_file(file_path)

                print()

        except Exception as e:
            print(f"Warning: Could not check for deleted files: {e}\n")

    def _print_summary(self) -> None:
        """Print indexing summary."""
        print(f"\n{'='*60}")
        print("INDEXING SUMMARY")
        print(f"{'='*60}")
        print(f"Files processed:     {self.files_processed}")
        print(f"Files skipped:       {self.files_skipped}")
        print(f"Total chunks added:  {self.total_chunks}")
        print(f"{'='*60}\n")


def run_indexing(
    directory: str,
    db_path: str = "./lancedb",
    clear: bool = False,
) -> Dict:
    """
    Run the indexing process programmatically.

    Args:
        directory: Directory to index
        db_path: Path to vector database
        clear: Whether to clear existing database before indexing

    Returns:
        Dictionary with indexing results:
        - success: bool
        - files_processed: int
        - files_skipped: int
        - chunks_added: int
        - total_chunks: int
        - unique_files: int
        - error: str (if success=False)

    Raises:
        ValueError: If directory is invalid
    """
    try:
        # Initialize vector store
        print("Initializing vector store...")
        store = VectorStore(db_path=db_path)

        # Clear if requested
        if clear:
            print("Clearing existing database...")
            store.clear()

        # Initialize indexer
        indexer = CodeIndexer(directory)

        # Run indexing
        indexer.index(store)

        # Get final stats
        stats = store.get_stats()
        print("Database Statistics:")
        print(f"  Total chunks: {stats.get('total_chunks', 0)}")
        print(f"  Unique files: {stats.get('unique_files', 0)}")
        print(f"  Database: {stats.get('db_path', 'N/A')}")

        return {
            "success": True,
            "files_processed": indexer.files_processed,
            "files_skipped": indexer.files_skipped,
            "chunks_added": indexer.total_chunks,
            "total_chunks": stats.get("total_chunks", 0),
            "unique_files": stats.get("unique_files", 0),
        }

    except ValueError as e:
        return {
            "success": False,
            "error": str(e),
        }
    except Exception as e:
        return {
            "success": False,
            "error": f"Unexpected error: {str(e)}",
        }


def main():
    """CLI entry point."""
    parser = argparse.ArgumentParser(
        description="Index code files for semantic search",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python indexer.py ~/code/MyProject
  python indexer.py /path/to/project
  python indexer.py .
        """,
    )

    parser.add_argument(
        "directory",
        help="Directory to index",
    )

    parser.add_argument(
        "--db-path",
        default="./lancedb",
        help="Path to vector database (default: ./lancedb)",
    )

    parser.add_argument(
        "--clear",
        action="store_true",
        help="Clear existing database before indexing",
    )

    args = parser.parse_args()

    try:
        result = run_indexing(
            directory=args.directory,
            db_path=args.db_path,
            clear=args.clear,
        )

        if not result["success"]:
            print(f"[ERROR] {result.get('error', 'Unknown error')}")
            sys.exit(1)

    except KeyboardInterrupt:
        print("\n\n[WARNING] Indexing interrupted by user")
        sys.exit(1)


if __name__ == "__main__":
    main()
