# RAG Code Search Setup

This system enables Claude CLI to semantically search your indexed codebase using a local vector database.

## Overview

The system consists of three components:

1. **`vector_store.py`** - Manages LanceDB vector database and embeddings
2. **`indexer.py`** - CLI tool to index your code projects
3. **`rag_server.py`** - MCP server that exposes search to Claude CLI

## Installation

### 1. Install Dependencies

```bash
pip install -r requirements.txt
```

This installs:
- `mcp` - MCP protocol for Claude integration
- `lancedb` (0.25.x) - Vector database with lance2.0 format
- `sentence-transformers` - Embedding model
- `torch`, `numpy`, `pandas` - Supporting libraries

**Notes:**
- First run will download the `all-MiniLM-L6-v2` model (~80MB)
- LanceDB 0.25.x is pinned for stability and uses the modern lance2.0 format
- Upgrading from 0.3.x? Your existing database will work with 0.25.x without migration

### 2. Index Your Codebase

```bash
# Index a project directory
python indexer.py /path/to/your/project

# Examples
python indexer.py ~/code/MyGame
python indexer.py .
python indexer.py C:\Projects\VerboseWaffle

# Clear existing database and reindex
python indexer.py --clear /path/to/project
```

**What gets indexed:**
- `.cs` (C# code)
- `.md` (Markdown documentation)
- `.txt` (Text files)

**What gets skipped:**
- `bin/`, `obj/`, `node_modules/`, `.git/`
- IDE folders (`.vs/`, `.vscode/`, `.idea/`)
- Godot cache (`.godot/`, `.import/`, `.mono/`)

**Chunking behavior:**
- C# code: Chunks at class/method boundaries (brace depth tracking)
- Text/Markdown: Chunks by paragraphs
- Minimum: 50 characters, 5 lines per chunk

### 3. Configure Claude CLI

Edit your MCP settings file:

**Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
**macOS/Linux:** `~/.config/claude/claude_desktop_config.json`

Add the RAG server:

```json
{
  "mcpServers": {
    "codebase-search": {
      "command": "python",
      "args": [
        "C:\\Users\\purli\\source\\cooking-project\\rag_server.py"
      ]
    }
  }
}
```

**Important:** Use absolute paths! Replace the path with your actual location.

### 4. Restart Claude CLI

Close and reopen Claude CLI to load the MCP server.

## Usage

Once configured, Claude can automatically search your codebase:

### Example Queries

**You:** "How is user authentication handled in this codebase?"

Claude will use the `search_codebase` tool to find relevant auth code.

**You:** "Show me the enemy AI behavior"

Claude will search for AI-related code chunks.

**You:** "Find database connection logic"

Claude will locate DB-related code.

### Manual Tool Usage

You can also explicitly ask Claude to search:

**You:** "Use the search_codebase tool to find player movement code"

**You:** "Search for top 10 results about 'save game functionality'"

## System Details

### Vector Store (`vector_store.py`)

**Database:** LanceDB stored in `./lancedb/`

**Embeddings:** `all-MiniLM-L6-v2` (384 dimensions)
- Fast, lightweight model
- Good balance of speed and quality
- Runs locally (no API calls)

**Schema:**
```python
{
    "vector": List[float],        # 384-dim embedding
    "content": str,               # Code chunk
    "source": str,                # File name
    "file_path": str,             # Full path
    "chunk_hash": str,            # SHA256 for deduplication
    "timestamp": str,             # ISO format
    "start_line": int,            # Starting line number
    "end_line": int               # Ending line number
}
```

**Deduplication:** Chunks are deduplicated by hash of (content + file_path)

### Indexer (`indexer.py`)

**CLI Options:**
```bash
python indexer.py [directory] [--db-path PATH] [--clear]
```

**Progress output:**
```
============================================================
Indexing directory: /path/to/project
============================================================

Finding files...
Found 42 files to index

[1/42] Processing: GameManager.cs
  ✓ Created 8 chunks
[2/42] Processing: PlayerController.cs
  ✓ Created 12 chunks
...

============================================================
Adding chunks to vector database...
============================================================

Processing 523 chunks...
  Processed 100/523 chunks...
  Processed 200/523 chunks...
  ...
Added 523 new chunks (skipped 0 duplicates)

============================================================
INDEXING SUMMARY
============================================================
Files processed:     42
Files skipped:       0
Total chunks added:  523
============================================================
```

### MCP Server (`rag_server.py`)

**Tool:** `search_codebase`

**Parameters:**
- `query` (required): Natural language search query
- `top_k` (optional): Number of results (default: 5, max: 20)

**Response format:**
```
Found 5 results for: 'player movement'

================================================================================

Result #1
File: /path/to/PlayerController.cs
Lines: 45-67
Relevance: 0.2341

```cs
public void MovePlayer(Vector2 direction)
{
    // Movement implementation
}
```

--------------------------------------------------------------------------------

...
```

**Error handling:**
- No data indexed → Helpful message with indexing instructions
- Empty query → Validation error
- Search failures → Detailed error with context

## Troubleshooting

### "No results found"

1. **Check if indexed:**
   ```python
   python -c "from vector_store import VectorStore; print(VectorStore().get_stats())"
   ```

2. **Re-index:**
   ```bash
   python indexer.py --clear /path/to/project
   ```

3. **Try different query terms**

### "Module not found" errors

```bash
pip install -r requirements.txt
```

### MCP server not appearing in Claude

1. Check config file path is correct
2. Use absolute paths in `args`
3. Restart Claude completely
4. Check logs (if available)

### Slow indexing

- First run downloads embedding model (~80MB)
- Large projects may take several minutes
- Progress is shown per file

### Database location

Default: `./lancedb/` in the current directory

To use a different location:
```bash
python indexer.py --db-path /custom/path /path/to/project
```

Update `rag_server.py` line 18:
```python
store = VectorStore(db_path="/custom/path")
```

## Maintenance

### Updating the index

When code changes significantly:

```bash
python indexer.py --clear /path/to/project
```

Or incremental update (adds new chunks, skips duplicates):

```bash
python indexer.py /path/to/project
```

### Viewing database stats

```python
from vector_store import VectorStore

store = VectorStore()
stats = store.get_stats()
print(f"Total chunks: {stats['total_chunks']}")
print(f"Unique files: {stats['unique_files']}")
print(f"Database: {stats['db_path']}")
```

### Testing search directly

```python
from vector_store import VectorStore

store = VectorStore()
results = store.search("player movement", top_k=3)

for r in results:
    print(f"{r['source']}: {r['content'][:100]}...")
```

## Architecture

```
┌─────────────┐
│  Claude CLI │
└─────┬───────┘
      │ MCP Protocol
      │
┌─────▼────────────┐
│  rag_server.py   │  (MCP Server)
│  - search_codebase tool
└─────┬────────────┘
      │
┌─────▼────────────┐
│ vector_store.py  │  (Vector DB Layer)
│  - LanceDB
│  - Embeddings
└──────────────────┘
      ▲
      │
┌─────┴────────────┐
│   indexer.py     │  (CLI Tool)
│  - File discovery
│  - Smart chunking
└──────────────────┘
```

## Performance

**Indexing:**
- ~10-50 files/second (depends on file size)
- ~100-500 chunks/second for embedding generation

**Search:**
- ~50-200ms per query (local, no API calls)
- Includes embedding generation + vector search

**Storage:**
- ~1KB per chunk (embedding + metadata)
- 1000 chunks ≈ 1MB database

## Advanced Usage

### Custom chunking

Edit `indexer.py` to change:
- `MIN_CHUNK_CHARS` (default: 50)
- `MIN_CHUNK_LINES` (default: 5)
- `SUPPORTED_EXTENSIONS` (add file types)
- `SKIP_DIRS` (skip additional directories)

### Different embedding model

Edit `vector_store.py` line 31:
```python
model_name: str = "all-MiniLM-L6-v2",
```

Other options:
- `all-mpnet-base-v2` (higher quality, slower)
- `paraphrase-MiniLM-L6-v2` (alternative)

### Filter by path

```python
results = store.search(
    "authentication",
    top_k=5,
    filter_path="/src/auth/"  # Only search in auth directory
)
```

## Security & Privacy

- ✅ **Fully local** - No data sent to external APIs
- ✅ **Private** - Embeddings generated locally
- ✅ **Offline** - Works without internet (after initial model download)
- ⚠️ **Database not encrypted** - Don't index sensitive credentials

## License

This RAG system integrates with the project's existing code quality standards
and is subject to the same license terms.
