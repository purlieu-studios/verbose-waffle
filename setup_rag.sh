#!/bin/bash
# Quick setup script for RAG system on macOS/Linux

echo "============================================================"
echo "RAG Code Search - Quick Setup"
echo "============================================================"
echo ""

echo "Step 1: Installing dependencies..."
pip install -r requirements.txt
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install dependencies"
    exit 1
fi

echo ""
echo "============================================================"
echo "Step 2: Ready to index!"
echo "============================================================"
echo ""
echo "To index your codebase, run:"
echo "  python indexer.py /path/to/your/project"
echo ""
echo "Example:"
echo "  python indexer.py ."
echo "  python indexer.py ~/code/MyGame"
echo ""
echo "============================================================"
echo "Step 3: Configure Claude CLI"
echo "============================================================"
echo ""
echo "Add this to your Claude config:"
echo "  File: ~/.config/claude/claude_desktop_config.json"
echo ""
echo '{'
echo '  "mcpServers": {'
echo '    "codebase-search": {'
echo '      "command": "python",'
echo '      "args": ["'$(pwd)'/rag_server.py"]'
echo '    }'
echo '  }'
echo '}'
echo ""
echo "Then restart Claude CLI."
echo ""
echo "============================================================"
echo "See RAG_SETUP.md for detailed instructions"
echo "============================================================"
