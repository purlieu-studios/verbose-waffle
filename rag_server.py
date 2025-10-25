#!/usr/bin/env python3
"""
MCP server for semantic code search.

Exposes the vector store as an MCP tool that Claude CLI can use
to search indexed codebases.
"""

import asyncio
import json
import sys
from typing import Any, Dict, List

from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent

from vector_store import VectorStore


# Initialize vector store globally
store = VectorStore()

# Create MCP server
app = Server("codebase-search")


@app.list_tools()
async def list_tools() -> List[Tool]:
    """
    List available MCP tools.

    Returns:
        List of available tools
    """
    return [
        Tool(
            name="search_codebase",
            description=(
                "Search indexed codebase for relevant code using semantic search. "
                "Returns code chunks with file locations and line numbers. "
                "Use this to find implementations, understand code structure, "
                "or locate specific functionality."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "query": {
                        "type": "string",
                        "description": (
                            "Natural language search query. Examples: "
                            "'authentication logic', 'database connection', "
                            "'enemy AI behavior', 'user input validation'"
                        ),
                    },
                    "top_k": {
                        "type": "number",
                        "description": "Number of results to return (default: 5, max: 20)",
                        "default": 5,
                        "minimum": 1,
                        "maximum": 20,
                    },
                },
                "required": ["query"],
            },
        )
    ]


@app.call_tool()
async def call_tool(name: str, arguments: Dict[str, Any]) -> List[TextContent]:
    """
    Handle tool calls from Claude.

    Args:
        name: Tool name
        arguments: Tool arguments

    Returns:
        List of text content responses

    Raises:
        ValueError: If tool name is unknown or arguments are invalid
    """
    if name != "search_codebase":
        raise ValueError(f"Unknown tool: {name}")

    # Extract arguments
    query = arguments.get("query", "").strip()
    if not query:
        raise ValueError("Query parameter is required and cannot be empty")

    top_k = int(arguments.get("top_k", 5))
    top_k = max(1, min(top_k, 20))  # Clamp to [1, 20]

    try:
        # Perform search
        results = store.search(query, top_k=top_k)

        if not results:
            return [
                TextContent(
                    type="text",
                    text=(
                        f"No results found for query: '{query}'\n\n"
                        "This could mean:\n"
                        "1. The codebase hasn't been indexed yet (run: python indexer.py /path/to/project)\n"
                        "2. No code matches this query\n"
                        "3. Try rephrasing your query with different keywords"
                    ),
                )
            ]

        # Format results
        formatted = format_search_results(query, results)

        return [TextContent(type="text", text=formatted)]

    except ValueError as e:
        # Handle vector store errors (e.g., no data indexed)
        return [
            TextContent(
                type="text",
                text=(
                    f"Error: {str(e)}\n\n"
                    "Make sure to index your codebase first:\n"
                    "  python indexer.py /path/to/your/project"
                ),
            )
        ]
    except Exception as e:
        # Unexpected errors
        error_msg = (
            f"Search error: {str(e)}\n\n"
            f"Query: {query}\n"
            f"Top K: {top_k}"
        )
        return [TextContent(type="text", text=error_msg)]


def format_search_results(query: str, results: List[Dict]) -> str:
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
    lines.append("- Use specific terms for better results (e.g., 'user authentication' vs 'code')")
    lines.append("- Increase top_k parameter to see more results")
    lines.append("- Results are ordered by relevance (lower score = more relevant)")

    return "\n".join(lines)


async def main():
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

    # Run the server
    async with stdio_server() as (read_stream, write_stream):
        await app.run(
            read_stream,
            write_stream,
            app.create_initialization_options(),
        )


if __name__ == "__main__":
    asyncio.run(main())
