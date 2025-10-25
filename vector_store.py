"""
Vector store management using LanceDB for semantic code search.

This module handles embedding generation and vector database operations
for storing and retrieving code chunks.
"""

import hashlib
import os
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional

import lancedb
import numpy as np
from sentence_transformers import SentenceTransformer


class VectorStore:
    """Manages vector embeddings and semantic search using LanceDB."""

    def __init__(
        self,
        db_path: str = "./lancedb",
        model_name: str = "all-MiniLM-L6-v2",
    ):
        """
        Initialize the vector store.

        Args:
            db_path: Path to LanceDB database directory
            model_name: SentenceTransformer model name for embeddings
        """
        self.db_path = Path(db_path)
        self.db_path.mkdir(parents=True, exist_ok=True)

        # Initialize embedding model
        print(f"Loading embedding model '{model_name}'...")
        self.model = SentenceTransformer(model_name)
        self.embedding_dim = self.model.get_sentence_embedding_dimension()

        # Connect to LanceDB
        self.db = lancedb.connect(str(self.db_path))
        self.table_name = "code_chunks"

        # Initialize or load table
        self._init_table()

    def _init_table(self) -> None:
        """Initialize the LanceDB table if it doesn't exist."""
        try:
            self.table = self.db.open_table(self.table_name)
            print(f"Loaded existing table '{self.table_name}' with {len(self.table)} chunks")
        except Exception:
            # Table doesn't exist, will be created on first add
            print(f"Table '{self.table_name}' will be created on first indexing")
            self.table = None

    def _generate_embedding(self, text: str) -> np.ndarray:
        """
        Generate embedding vector for text.

        Args:
            text: Text to embed

        Returns:
            Embedding vector as numpy array
        """
        return self.model.encode(text, convert_to_numpy=True)

    def _compute_hash(self, content: str, file_path: str) -> str:
        """
        Compute unique hash for a chunk.

        Args:
            content: Chunk content
            file_path: Source file path

        Returns:
            SHA256 hash string
        """
        data = f"{file_path}:{content}"
        return hashlib.sha256(data.encode()).hexdigest()

    def add_chunks(self, chunks: List[Dict]) -> int:
        """
        Add code chunks to the vector store.

        Args:
            chunks: List of chunk dictionaries with keys:
                - content: str (code content)
                - source: str (file name)
                - file_path: str (full path)
                - start_line: int (optional)
                - end_line: int (optional)

        Returns:
            Number of chunks added

        Raises:
            ValueError: If chunks list is empty or invalid
        """
        if not chunks:
            raise ValueError("Chunks list cannot be empty")

        print(f"Processing {len(chunks)} chunks...")

        # Prepare data for insertion
        records = []
        for i, chunk in enumerate(chunks):
            if i % 100 == 0 and i > 0:
                print(f"  Processed {i}/{len(chunks)} chunks...")

            content = chunk.get("content", "")
            file_path = chunk.get("file_path", "")

            if not content or not file_path:
                continue

            # Generate embedding
            vector = self._generate_embedding(content)

            # Compute hash for deduplication
            chunk_hash = self._compute_hash(content, file_path)

            record = {
                "vector": vector.tolist(),
                "content": content,
                "source": chunk.get("source", Path(file_path).name),
                "file_path": file_path,
                "chunk_hash": chunk_hash,
                "timestamp": datetime.now().isoformat(),
                "start_line": chunk.get("start_line", 0),
                "end_line": chunk.get("end_line", 0),
            }
            records.append(record)

        if not records:
            return 0

        # Create or append to table
        if self.table is None:
            self.table = self.db.create_table(self.table_name, data=records)
            print(f"Created table '{self.table_name}'")
        else:
            # Remove duplicates based on chunk_hash
            existing_hashes = set()
            try:
                for row in self.table.to_pandas()["chunk_hash"]:
                    existing_hashes.add(row)
            except Exception:
                pass

            new_records = [
                r for r in records if r["chunk_hash"] not in existing_hashes
            ]

            if new_records:
                self.table.add(new_records)
                print(f"Added {len(new_records)} new chunks (skipped {len(records) - len(new_records)} duplicates)")
            else:
                print("All chunks already exist in database")

            return len(new_records)

        return len(records)

    def search(
        self,
        query: str,
        top_k: int = 5,
        filter_path: Optional[str] = None,
    ) -> List[Dict]:
        """
        Perform semantic search on code chunks.

        Args:
            query: Search query
            top_k: Number of results to return
            filter_path: Optional path prefix to filter results

        Returns:
            List of matching chunks with scores

        Raises:
            ValueError: If query is empty or table doesn't exist
        """
        if not query:
            raise ValueError("Query cannot be empty")

        if self.table is None:
            raise ValueError("No data indexed yet. Run indexer.py first.")

        # Generate query embedding
        query_vector = self._generate_embedding(query)

        # Perform vector search
        results = (
            self.table.search(query_vector.tolist())
            .limit(top_k)
            .to_pandas()
        )

        # Format results
        matches = []
        for _, row in results.iterrows():
            # Apply path filter if specified
            if filter_path and not row["file_path"].startswith(filter_path):
                continue

            match = {
                "content": row["content"],
                "source": row["source"],
                "file_path": row["file_path"],
                "score": float(row.get("_distance", 0.0)),
                "start_line": int(row.get("start_line", 0)),
                "end_line": int(row.get("end_line", 0)),
            }
            matches.append(match)

        return matches[:top_k]

    def clear(self) -> None:
        """Clear all data from the vector store."""
        try:
            self.db.drop_table(self.table_name)
            self.table = None
            print(f"Cleared table '{self.table_name}'")
        except Exception as e:
            print(f"Warning: Could not clear table: {e}")

    def get_stats(self) -> Dict:
        """
        Get statistics about the vector store.

        Returns:
            Dictionary with stats
        """
        if self.table is None:
            return {
                "total_chunks": 0,
                "db_path": str(self.db_path),
                "model": self.model.get_sentence_embedding_dimension(),
            }

        try:
            df = self.table.to_pandas()
            return {
                "total_chunks": len(df),
                "unique_files": df["file_path"].nunique(),
                "db_path": str(self.db_path),
                "embedding_dim": self.embedding_dim,
            }
        except Exception as e:
            return {
                "total_chunks": 0,
                "error": str(e),
                "db_path": str(self.db_path),
            }


if __name__ == "__main__":
    # Test the vector store
    store = VectorStore()

    # Test data
    test_chunks = [
        {
            "content": "public class GameManager { }",
            "source": "GameManager.cs",
            "file_path": "/test/GameManager.cs",
            "start_line": 1,
            "end_line": 1,
        }
    ]

    store.add_chunks(test_chunks)
    print("\nStats:", store.get_stats())

    results = store.search("game management", top_k=1)
    print("\nSearch results:", len(results))
    for r in results:
        print(f"  - {r['source']}: {r['content'][:50]}...")
