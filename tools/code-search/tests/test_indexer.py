"""
Tests for the code indexer module.
"""

import pytest
from pathlib import Path
from unittest.mock import Mock, patch, MagicMock
import tempfile
import os

# Import the module we're testing
import sys

sys.path.insert(0, str(Path(__file__).parent.parent))
from indexer import CodeIndexer


class TestCodeIndexerInitialization:
    """Tests for CodeIndexer initialization."""

    def test_init_with_valid_directory(self, tmp_path):
        """Test initialization with valid directory."""
        indexer = CodeIndexer(str(tmp_path))

        assert indexer.root_path == tmp_path.resolve()
        assert indexer.files_processed == 0
        assert indexer.files_skipped == 0
        assert indexer.total_chunks == 0

    def test_init_with_nonexistent_path(self):
        """Test that nonexistent path raises ValueError."""
        with pytest.raises(ValueError, match="Path does not exist"):
            CodeIndexer("/nonexistent/path/12345")

    def test_init_with_file_instead_of_directory(self, tmp_path):
        """Test that file path raises ValueError."""
        test_file = tmp_path / "test.txt"
        test_file.write_text("content")

        with pytest.raises(ValueError, match="Path is not a directory"):
            CodeIndexer(str(test_file))


class TestDirectoryFiltering:
    """Tests for directory and file filtering."""

    def test_should_skip_directory(self, tmp_path):
        """Test directory skipping logic."""
        indexer = CodeIndexer(str(tmp_path))

        # Should skip
        assert indexer.should_skip_directory(Path("/test/bin"))
        assert indexer.should_skip_directory(Path("/test/obj"))
        assert indexer.should_skip_directory(Path("/test/node_modules"))
        assert indexer.should_skip_directory(Path("/test/.git"))
        assert indexer.should_skip_directory(Path("/test/__pycache__"))

        # Should not skip
        assert not indexer.should_skip_directory(Path("/test/src"))
        assert not indexer.should_skip_directory(Path("/test/lib"))

    def test_is_supported_file(self, tmp_path):
        """Test file extension filtering."""
        indexer = CodeIndexer(str(tmp_path))

        # Supported files
        assert indexer.is_supported_file(Path("test.cs"))
        assert indexer.is_supported_file(Path("test.CS"))  # Case insensitive
        assert indexer.is_supported_file(Path("test.md"))
        assert indexer.is_supported_file(Path("test.txt"))

        # Unsupported files
        assert not indexer.is_supported_file(Path("test.py"))
        assert not indexer.is_supported_file(Path("test.js"))
        assert not indexer.is_supported_file(Path("test.exe"))

    def test_find_files_recursively(self, tmp_path):
        """Test finding files recursively."""
        # Create test structure
        (tmp_path / "src").mkdir()
        (tmp_path / "src" / "test.cs").write_text("class Test {}")
        (tmp_path / "src" / "README.md").write_text("# Test")
        (tmp_path / "bin").mkdir()
        (tmp_path / "bin" / "test.dll").write_text("binary")
        (tmp_path / "test.txt").write_text("text")

        indexer = CodeIndexer(str(tmp_path))
        files = indexer.find_files()

        file_names = [f.name for f in files]
        assert "test.cs" in file_names
        assert "README.md" in file_names
        assert "test.txt" in file_names
        assert "test.dll" not in file_names  # Wrong extension
        # bin directory should be skipped


class TestCodeChunking:
    """Tests for code chunking functionality."""

    def test_chunk_csharp_code_by_braces(self, tmp_path):
        """Test C# code is chunked at brace boundaries."""
        indexer = CodeIndexer(str(tmp_path))

        code = """using System;

public class GameManager
{
    private int score = 0;

    public void AddScore(int points)
    {
        score += points;
    }
}

public class Player
{
    public string Name { get; set; }
}"""

        chunks = indexer.chunk_code(code, Path("test.cs"))

        assert len(chunks) >= 2
        # First chunk should contain GameManager
        assert "GameManager" in chunks[0]["content"]
        assert chunks[0]["source"] == "test.cs"
        assert chunks[0]["start_line"] > 0

    def test_chunk_text_file_by_paragraphs(self, tmp_path):
        """Test text files are chunked by paragraphs."""
        indexer = CodeIndexer(str(tmp_path))

        text = """This is the first paragraph.
It has multiple lines.

This is the second paragraph.
It also has multiple lines.

This is the third paragraph."""

        chunks = indexer.chunk_text(text, Path("test.md"))

        assert len(chunks) == 3
        assert "first paragraph" in chunks[0]["content"]
        assert "second paragraph" in chunks[1]["content"]
        assert "third paragraph" in chunks[2]["content"]

    def test_chunk_respects_minimum_size(self, tmp_path):
        """Test that small chunks are filtered out."""
        indexer = CodeIndexer(str(tmp_path))

        # Very small code
        code = "int x;"

        chunks = indexer.chunk_code(code, Path("test.cs"))

        # Should produce no chunks (too small)
        assert len(chunks) == 0

    def test_chunk_empty_content(self, tmp_path):
        """Test chunking empty content returns empty list."""
        indexer = CodeIndexer(str(tmp_path))

        chunks = indexer.chunk_code("", Path("test.cs"))

        assert chunks == []


class TestFileProcessing:
    """Tests for file processing."""

    def test_process_file_success(self, tmp_path):
        """Test successfully processing a file."""
        test_file = tmp_path / "test.cs"
        test_file.write_text(
            """
public class TestClass
{
    public void TestMethod()
    {
        Console.WriteLine("Hello");
    }
}
"""
        )

        indexer = CodeIndexer(str(tmp_path))
        chunks = indexer.process_file(test_file)

        assert len(chunks) > 0
        assert chunks[0]["file_path"] == str(test_file)
        assert "TestClass" in chunks[0]["content"]

    def test_process_empty_file(self, tmp_path):
        """Test processing empty file returns no chunks."""
        test_file = tmp_path / "empty.cs"
        test_file.write_text("")

        indexer = CodeIndexer(str(tmp_path))
        chunks = indexer.process_file(test_file)

        assert chunks == []

    def test_process_file_handles_encoding_errors(self, tmp_path):
        """Test file with encoding errors is handled gracefully."""
        test_file = tmp_path / "binary.cs"
        # Write some binary content
        test_file.write_bytes(b"\x80\x81\x82\x83")

        indexer = CodeIndexer(str(tmp_path))
        # Should not raise, just handle gracefully
        chunks = indexer.process_file(test_file)

        # May be empty or contain partial content
        assert isinstance(chunks, list)


class TestPrivateHelpers:
    """Tests for private helper methods."""

    def test_chunk_text_with_short_paragraphs(self, tmp_path):
        """Test text chunking filters out short paragraphs."""
        indexer = CodeIndexer(str(tmp_path))

        text = """Short.

This is a much longer paragraph that should definitely be included in the chunks because it meets the minimum character requirements.

x"""

        chunks = indexer._chunk_text(text, Path("test.md"))

        # Should only get the long paragraph
        assert len(chunks) == 1
        assert "longer paragraph" in chunks[0]["content"]

    def test_chunk_code_tracks_line_numbers(self, tmp_path):
        """Test that chunks track correct line numbers."""
        indexer = CodeIndexer(str(tmp_path))

        code = """// Line 1
public class Test  // Line 2
{  // Line 3
    public void Method()  // Line 4
    {  // Line 5
        var x = 1;  // Line 6
    }  // Line 7
}  // Line 8"""

        chunks = indexer.chunk_code(code, Path("test.cs"))

        assert len(chunks) > 0
        chunk = chunks[0]
        assert chunk["start_line"] == 1
        assert chunk["end_line"] > chunk["start_line"]
