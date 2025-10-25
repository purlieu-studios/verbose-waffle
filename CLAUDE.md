# Working with Claude Code

This project has semantic code search enabled via an MCP (Model Context Protocol) server.

## How Code Search Works

### Automatic Search
Claude will automatically search your codebase when you ask questions like:
- "How does authentication work?"
- "Where are the database queries?"
- "Show me the API endpoints"
- "How is error handling implemented?"

You don't need to do anything - just ask questions naturally.

### Reindexing the Codebase

The code search uses a vector database index. After making significant code changes, the index should be updated.

**When to reindex:**
- After adding new files or features
- After refactoring existing code
- When search results seem stale or missing recent changes
- When starting work on the project for the first time

**How to reindex:**
Simply ask Claude: "Please reindex the codebase"

Claude can run this for you when appropriate, or you can request it after making changes.

## MCP Server Details

The code search is powered by:
- **Server:** `code-search` (defined in `.mcp.json`)
- **Tools:**
  - `search_codebase` - Semantic search across your code
  - `reindex_codebase` - Update the search index
- **Database:** LanceDB vector database stored in `lancedb/`
