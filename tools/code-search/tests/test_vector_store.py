"""
Tests for the vector store module.
"""

import numpy as np
import pandas as pd
import pytest
from unittest.mock import Mock, MagicMock, patch, call
from pathlib import Path

# Import the module we're testing
import sys

sys.path.insert(0, str(Path(__file__).parent.parent))
from vector_store import VectorStore


class TestVectorStoreInitialization:
    """Tests for VectorStore initialization."""

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_init_creates_db_directory(self, mock_connect, mock_model):
        """Test that initialization creates the database directory."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_db = Mock()
        mock_db.open_table.side_effect = Exception("Table doesn't exist")
        mock_connect.return_value = mock_db

        store = VectorStore(db_path="./test_db")

        assert store.db_path == Path("./test_db")
        assert store.embedding_dim == 384
        mock_connect.assert_called_once_with("./test_db")

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_init_loads_existing_table(self, mock_connect, mock_model):
        """Test that initialization loads an existing table."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_table = Mock()
        mock_table.__len__ = Mock(return_value=100)

        mock_db = Mock()
        mock_db.open_table.return_value = mock_table
        mock_connect.return_value = mock_db

        store = VectorStore()

        assert store.table == mock_table
        mock_db.open_table.assert_called_once_with("code_chunks")


class TestVectorStoreAddChunks:
    """Tests for adding chunks to the vector store."""

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_add_chunks_to_new_table(self, mock_connect, mock_model):
        """Test adding chunks when table doesn't exist."""
        # Setup mocks
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model_instance.encode.return_value = np.array([0.1] * 384)
        mock_model.return_value = mock_model_instance

        mock_db = Mock()
        mock_db.open_table.side_effect = Exception("No table")
        mock_table = Mock()
        mock_db.create_table.return_value = mock_table
        mock_connect.return_value = mock_db

        store = VectorStore()

        chunks = [
            {
                "content": "def hello(): pass",
                "file_path": "/test/hello.py",
                "source": "hello.py",
                "start_line": 1,
                "end_line": 1,
            }
        ]

        result = store.add_chunks(chunks)

        assert result == 1
        mock_db.create_table.assert_called_once()
        assert store.table == mock_table

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_add_chunks_validates_input(self, mock_connect, mock_model):
        """Test that empty chunks list raises ValueError."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_db = Mock()
        mock_db.open_table.side_effect = Exception("No table")
        mock_connect.return_value = mock_db

        store = VectorStore()

        with pytest.raises(ValueError, match="Chunks list cannot be empty"):
            store.add_chunks([])

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_add_chunks_with_update_existing(self, mock_connect, mock_model):
        """Test adding chunks with update_existing=True removes old chunks."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model_instance.encode.return_value = np.array([0.1] * 384)
        mock_model.return_value = mock_model_instance

        mock_table = Mock()
        mock_table.add = Mock()

        mock_db = Mock()
        mock_db.open_table.return_value = mock_table
        mock_connect.return_value = mock_db

        store = VectorStore()
        store.remove_chunks_by_file = Mock(return_value=5)

        chunks = [
            {
                "content": "new content",
                "file_path": "/test/file.py",
                "source": "file.py",
            }
        ]

        result = store.add_chunks(chunks, update_existing=True)

        store.remove_chunks_by_file.assert_called_once_with("/test/file.py")
        mock_table.add.assert_called_once()
        assert result == 1


class TestVectorStoreSearch:
    """Tests for searching the vector store."""

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_search_returns_results(self, mock_connect, mock_model):
        """Test that search returns formatted results."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model_instance.encode.return_value = np.array([0.1] * 384)
        mock_model.return_value = mock_model_instance

        # Mock search results
        mock_search_results = pd.DataFrame(
            [
                {
                    "content": "test content",
                    "source": "test.py",
                    "file_path": "/test/test.py",
                    "_distance": 0.5,
                    "start_line": 1,
                    "end_line": 10,
                }
            ]
        )

        mock_search = Mock()
        mock_search.limit.return_value.to_pandas.return_value = mock_search_results

        mock_table = Mock()
        mock_table.search.return_value = mock_search

        mock_db = Mock()
        mock_db.open_table.return_value = mock_table
        mock_connect.return_value = mock_db

        store = VectorStore()

        results = store.search("test query", top_k=5)

        assert len(results) == 1
        assert results[0]["content"] == "test content"
        assert results[0]["source"] == "test.py"
        assert results[0]["score"] == 0.5

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_search_validates_query(self, mock_connect, mock_model):
        """Test that empty query raises ValueError."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_table = Mock()
        mock_db = Mock()
        mock_db.open_table.return_value = mock_table
        mock_connect.return_value = mock_db

        store = VectorStore()

        with pytest.raises(ValueError, match="Query cannot be empty"):
            store.search("")

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_search_raises_when_no_data(self, mock_connect, mock_model):
        """Test that search raises error when no data indexed."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_db = Mock()
        mock_db.open_table.side_effect = Exception("No table")
        mock_connect.return_value = mock_db

        store = VectorStore()

        with pytest.raises(ValueError, match="No data indexed yet"):
            store.search("test query")


class TestVectorStoreManagement:
    """Tests for vector store management operations."""

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_remove_chunks_by_file(self, mock_connect, mock_model):
        """Test removing chunks from a specific file."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        # Mock table with data
        test_data = pd.DataFrame(
            [
                {"file_path": "/test/file1.py", "content": "content1"},
                {"file_path": "/test/file2.py", "content": "content2"},
                {"file_path": "/test/file1.py", "content": "content3"},
            ]
        )

        mock_table = Mock()
        mock_table.to_pandas.return_value = test_data

        mock_db = Mock()
        mock_db.open_table.return_value = mock_table
        mock_db.drop_table = Mock()
        mock_db.create_table = Mock(return_value=mock_table)
        mock_connect.return_value = mock_db

        store = VectorStore()

        removed = store.remove_chunks_by_file("/test/file1.py")

        assert removed == 2
        mock_db.drop_table.assert_called_once_with("code_chunks")
        mock_db.create_table.assert_called_once()

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_clear_drops_table(self, mock_connect, mock_model):
        """Test that clear drops the table."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_table = Mock()
        mock_db = Mock()
        mock_db.open_table.return_value = mock_table
        mock_db.drop_table = Mock()
        mock_connect.return_value = mock_db

        store = VectorStore()
        store.clear()

        mock_db.drop_table.assert_called_once_with("code_chunks")
        assert store.table is None

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_get_stats_with_data(self, mock_connect, mock_model):
        """Test getting statistics when data exists."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        test_data = pd.DataFrame(
            [
                {"file_path": "/test/file1.py", "content": "content1"},
                {"file_path": "/test/file2.py", "content": "content2"},
                {"file_path": "/test/file1.py", "content": "content3"},
            ]
        )

        mock_table = Mock()
        mock_table.to_pandas.return_value = test_data

        mock_db = Mock()
        mock_db.open_table.return_value = mock_table
        mock_connect.return_value = mock_db

        store = VectorStore(db_path="./test_db")

        stats = store.get_stats()

        assert stats["total_chunks"] == 3
        assert stats["unique_files"] == 2
        assert stats["embedding_dim"] == 384
        assert "test_db" in stats["db_path"]

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_get_stats_without_data(self, mock_connect, mock_model):
        """Test getting statistics when no data exists."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_db = Mock()
        mock_db.open_table.side_effect = Exception("No table")
        mock_connect.return_value = mock_db

        store = VectorStore()

        stats = store.get_stats()

        assert stats["total_chunks"] == 0
        assert stats["model"] == 384


class TestVectorStoreHelperMethods:
    """Tests for helper methods."""

    @patch("vector_store.SentenceTransformer")
    @patch("vector_store.lancedb.connect")
    def test_compute_hash(self, mock_connect, mock_model):
        """Test hash computation is consistent."""
        mock_model_instance = Mock()
        mock_model_instance.get_sentence_embedding_dimension.return_value = 384
        mock_model.return_value = mock_model_instance

        mock_db = Mock()
        mock_db.open_table.side_effect = Exception("No table")
        mock_connect.return_value = mock_db

        store = VectorStore()

        hash1 = store._compute_hash("content", "/test/file.py")
        hash2 = store._compute_hash("content", "/test/file.py")
        hash3 = store._compute_hash("different", "/test/file.py")

        assert hash1 == hash2  # Same input = same hash
        assert hash1 != hash3  # Different input = different hash
