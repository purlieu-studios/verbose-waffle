"""
Tests for the RAG MCP server.
"""

import pytest
from unittest.mock import Mock, patch, MagicMock
from pathlib import Path

# Import the module we're testing
import sys

sys.path.insert(0, str(Path(__file__).parent.parent))


class TestSearchCodebaseTool:
    """Tests for the search_codebase MCP tool."""

    @patch("rag_server.store")
    def test_search_with_valid_query(self, mock_store):
        """Test successful search with valid query."""
        # Import after patching
        from rag_server import search_codebase

        # Mock search results
        mock_store.search.return_value = [
            {
                "content": "public class GameManager { }",
                "source": "GameManager.cs",
                "file_path": "/test/GameManager.cs",
                "score": 0.85,
                "start_line": 1,
                "end_line": 10,
            }
        ]

        result = search_codebase("game manager", top_k=5)

        assert "GameManager" in result
        assert "GameManager.cs" in result
        mock_store.search.assert_called_once_with("game manager", top_k=5)

    @patch("rag_server.store")
    def test_search_with_empty_query(self, mock_store):
        """Test that empty query returns error message."""
        from rag_server import search_codebase

        result = search_codebase("   ", top_k=5)

        assert "Error" in result
        assert "empty" in result.lower()
        mock_store.search.assert_not_called()

    @patch("rag_server.store")
    def test_search_clamps_top_k(self, mock_store):
        """Test that top_k is clamped to valid range."""
        from rag_server import search_codebase

        mock_store.search.return_value = []

        # Test maximum clamp
        search_codebase("test", top_k=100)
        mock_store.search.assert_called_with("test", top_k=20)

        # Test minimum clamp
        search_codebase("test", top_k=0)
        mock_store.search.assert_called_with("test", top_k=1)

        # Test None default
        search_codebase("test", top_k=None)
        mock_store.search.assert_called_with("test", top_k=5)

    @patch("rag_server.store")
    def test_search_with_no_results(self, mock_store):
        """Test search when no results found."""
        from rag_server import search_codebase

        mock_store.search.return_value = []

        result = search_codebase("nonexistent code")

        assert "No results found" in result
        assert "indexed" in result.lower()

    @patch("rag_server.store")
    def test_search_handles_value_error(self, mock_store):
        """Test search handles ValueError from store."""
        from rag_server import search_codebase

        mock_store.search.side_effect = ValueError("No data indexed yet")

        result = search_codebase("test query")

        assert "Error" in result
        assert "No data indexed yet" in result
        assert "indexer.py" in result

    @patch("rag_server.store")
    def test_search_handles_general_exception(self, mock_store):
        """Test search handles unexpected exceptions."""
        from rag_server import search_codebase

        mock_store.search.side_effect = Exception("Unexpected error")

        result = search_codebase("test query")

        assert "Search error" in result
        assert "Unexpected error" in result


class TestReindexCodebaseTool:
    """Tests for the reindex_codebase MCP tool."""

    @patch("rag_server.run_indexing")
    @patch("rag_server.store")
    def test_reindex_with_defaults(self, mock_store, mock_run_indexing):
        """Test reindex with default parameters."""
        from rag_server import reindex_codebase

        mock_run_indexing.return_value = {
            "files_processed": 10,
            "files_skipped": 2,
            "chunks_added": 50,
        }

        result = reindex_codebase()

        assert "10" in result  # files processed
        assert "50" in result  # chunks added
        mock_store.clear.assert_not_called()  # clear=False by default
        mock_run_indexing.assert_called_once()

    @patch("rag_server.run_indexing")
    @patch("rag_server.store")
    def test_reindex_with_clear_true(self, mock_store, mock_run_indexing):
        """Test reindex with clear=True clears database first."""
        from rag_server import reindex_codebase

        mock_run_indexing.return_value = {
            "files_processed": 5,
            "files_skipped": 0,
            "chunks_added": 25,
        }

        result = reindex_codebase(clear=True)

        mock_store.clear.assert_called_once()
        assert "Clearing" in result or "cleared" in result.lower()

    @patch("rag_server.run_indexing")
    @patch("rag_server.store")
    def test_reindex_with_custom_directory(self, mock_store, mock_run_indexing):
        """Test reindex with custom directory."""
        from rag_server import reindex_codebase

        mock_run_indexing.return_value = {
            "files_processed": 3,
            "files_skipped": 1,
            "chunks_added": 15,
        }

        result = reindex_codebase(directory="/custom/path")

        # Should pass directory to run_indexing
        args, kwargs = mock_run_indexing.call_args
        assert "/custom/path" in str(args) or "/custom/path" in str(kwargs)

    @patch("rag_server.run_indexing")
    @patch("rag_server.store")
    def test_reindex_handles_exception(self, mock_store, mock_run_indexing):
        """Test reindex handles indexing errors."""
        from rag_server import reindex_codebase

        mock_run_indexing.side_effect = Exception("Indexing failed")

        result = reindex_codebase()

        assert "Error" in result
        assert "Indexing failed" in result

    @patch("rag_server.run_indexing")
    @patch("rag_server.store")
    def test_reindex_with_empty_directory(self, mock_store, mock_run_indexing):
        """Test reindex defaults empty directory to current dir."""
        from rag_server import reindex_codebase

        mock_run_indexing.return_value = {
            "files_processed": 0,
            "files_skipped": 0,
            "chunks_added": 0,
        }

        result = reindex_codebase(directory="")

        # Should default to "."
        args, kwargs = mock_run_indexing.call_args
        assert "." in str(args) or "." in str(kwargs)


class TestFormatSearchResults:
    """Tests for result formatting."""

    def test_format_search_results_helper(self):
        """Test the format_search_results helper function."""
        from rag_server import format_search_results

        results = [
            {
                "content": "public class Test { }",
                "source": "Test.cs",
                "file_path": "/path/Test.cs",
                "score": 0.9,
                "start_line": 1,
                "end_line": 5,
            }
        ]

        formatted = format_search_results("test query", results)

        assert "test query" in formatted
        assert "Test.cs" in formatted
        assert "public class Test" in formatted
        assert "1-5" in formatted or "Lines: 1" in formatted


class TestMCPServerInitialization:
    """Tests for MCP server setup."""

    @patch("rag_server.VectorStore")
    def test_server_initializes_vector_store(self, mock_vector_store):
        """Test that server initializes vector store on startup."""
        # Reimport to trigger initialization
        import importlib
        import rag_server

        importlib.reload(rag_server)

        # VectorStore should be instantiated
        assert mock_vector_store.called or hasattr(rag_server, "store")

    def test_mcp_tools_are_registered(self):
        """Test that MCP tools are properly registered."""
        from rag_server import mcp

        # Check that tools are available
        tool_names = [tool.name for tool in mcp.list_tools()]

        assert "search_codebase" in tool_names
        assert "reindex_codebase" in tool_names
