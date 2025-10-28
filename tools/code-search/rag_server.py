#!/usr/bin/env python3
"""
MCP server for semantic code search.

Exposes the vector store as an MCP tool that Claude CLI can use
to search indexed codebases.
"""

import sys
from typing import Optional

from mcp.server.fastmcp import FastMCP

from vector_store import VectorStore
from indexer import run_indexing


# Initialize vector store globally
store = VectorStore()

# Create FastMCP server
mcp = FastMCP("codebase-search")


@mcp.tool()
def search_codebase(query: str, top_k: Optional[int] = 5) -> str:
    """
    Search indexed codebase for relevant code using semantic search.

    Returns code chunks with file locations and line numbers.
    Use this to find implementations, understand code structure,
    or locate specific functionality.

    Args:
        query: Natural language search query. Examples: 'authentication logic',
               'database connection', 'enemy AI behavior', 'user input validation'
        top_k: Number of results to return (default: 5, max: 20)

    Returns:
        Formatted search results with code chunks and file locations
    """
    # Validate and clamp top_k
    if top_k is None:
        top_k = 5
    top_k = max(1, min(int(top_k), 20))

    # Validate query
    query = query.strip()
    if not query:
        return "Error: Query parameter is required and cannot be empty"

    try:
        # Perform search
        results = store.search(query, top_k=top_k)

        if not results:
            return (
                f"No results found for query: '{query}'\n\n"
                "This could mean:\n"
                "1. The codebase hasn't been indexed yet (run: python indexer.py /path/to/project)\n"
                "2. No code matches this query\n"
                "3. Try rephrasing your query with different keywords"
            )

        # Format and return results
        return format_search_results(query, results)

    except ValueError as e:
        return (
            f"Error: {str(e)}\n\n"
            "Make sure to index your codebase first:\n"
            "  python indexer.py /path/to/your/project"
        )
    except Exception as e:
        return f"Search error: {str(e)}\n\n" f"Query: {query}\n" f"Top K: {top_k}"


@mcp.tool()
def reindex_codebase(
    directory: Optional[str] = ".", clear: Optional[bool] = False
) -> str:
    """
    Reindex the codebase to update the vector database with latest code changes.

    Use this when you've made significant changes to the codebase and want to
    ensure search results reflect the current state of the code.

    Args:
        directory: Directory to index (default: current directory ".")
        clear: Whether to clear the existing database before reindexing (default: False)
               Set to True for a fresh start, False to add/update incrementally

    Returns:
        Formatted status message with indexing results
    """
    # Validate directory
    if not directory:
        directory = "."

    # Convert to absolute path for better clarity
    import os

    abs_directory = os.path.abspath(directory)

    # Build status header
    action = "Clearing and reindexing" if clear else "Reindexing"
    lines = [
        f"{'='*70}",
        f"{action}: {abs_directory}",
        f"{'='*70}",
        "",
    ]

    # Run the indexing
    try:
        result = run_indexing(directory=directory, db_path="./lancedb", clear=clear)
    except Exception as e:
        lines.extend(
            [
                f"❌ Error during indexing: {str(e)}",
                "",
                f"{'='*70}",
            ]
        )
        return "\n".join(lines)

    if not result["success"]:
        error_msg = result.get("error", "Unknown error")
        lines.extend(
            [
                f"❌ Indexing failed: {error_msg}",
                "",
                "Common issues:",
                "- Directory doesn't exist or is not accessible",
                "- No supported files found (.cs, .md, .txt)",
                "- Permission denied",
                "",
                f"{'='*70}",
            ]
        )
        return "\n".join(lines)

    # Success - format results
    lines.extend(
        [
            "✅ Indexing completed successfully!",
            "",
            "Results:",
            f"  • Files processed:  {result['files_processed']}",
            f"  • Files skipped:    {result['files_skipped']}",
            f"  • Chunks added:     {result['chunks_added']}",
            "",
            "Database Statistics:",
            f"  • Total chunks:     {result['total_chunks']}",
            f"  • Unique files:     {result['unique_files']}",
            "",
            f"{'='*70}",
            "",
            "💡 Tip: Use search_codebase to test the updated index!",
        ]
    )

    return "\n".join(lines)


def format_search_results(query: str, results: list[dict]) -> str:
    """
    Format search results for display.

    Args:
        query: Original search query
        results: List of search result dictionaries

    Returns:
        Formatted string
    """
    lines = [
        f"Found {len(results)} results for: '{query}'",
        "",
        "=" * 80,
        "",
    ]

    for i, result in enumerate(results, 1):
        file_path = result.get("file_path", "unknown")
        source = result.get("source", "unknown")
        content = result.get("content", "")
        score = result.get("score", 0.0)
        start_line = result.get("start_line", 0)
        end_line = result.get("end_line", 0)

        # Header
        lines.append(f"Result #{i}")
        lines.append(f"File: {file_path}")

        if start_line and end_line:
            lines.append(f"Lines: {start_line}-{end_line}")

        lines.append(f"Relevance: {score:.4f}")
        lines.append("")

        # Content with syntax highlighting hint
        file_ext = file_path.split(".")[-1] if "." in file_path else "txt"
        lines.append(f"```{file_ext}")
        lines.append(content)
        lines.append("```")
        lines.append("")
        lines.append("-" * 80)
        lines.append("")

    # Add summary
    lines.append("=" * 80)
    lines.append("")
    lines.append("Search Tips:")
    lines.append(
        "- Use specific terms for better results (e.g., 'user authentication' vs 'code')"
    )
    lines.append("- Increase top_k parameter to see more results")
    lines.append("- Results are ordered by relevance (lower score = more relevant)")

    return "\n".join(lines)


def main():
    """Run the MCP server."""
    # Check if vector store has data
    try:
        stats = store.get_stats()
        total_chunks = stats.get("total_chunks", 0)

        if total_chunks == 0:
            print(
                "WARNING: No data indexed yet. Run 'python indexer.py /path/to/project' first.",
                file=sys.stderr,
            )
        else:
            print(
                f"MCP Server ready. Indexed: {total_chunks} chunks from {stats.get('unique_files', 0)} files",
                file=sys.stderr,
            )

    except Exception as e:
        print(f"Warning: Could not get vector store stats: {e}", file=sys.stderr)

    # Run the FastMCP server
    mcp.run()


if __name__ == "__main__":
    main()
