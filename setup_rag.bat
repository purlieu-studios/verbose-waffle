@echo off
REM Quick setup script for RAG system on Windows

echo ============================================================
echo RAG Code Search - Quick Setup
echo ============================================================
echo.

echo Step 1: Installing dependencies...
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo ERROR: Failed to install dependencies
    pause
    exit /b 1
)

echo.
echo ============================================================
echo Step 2: Ready to index!
echo ============================================================
echo.
echo To index your codebase, run:
echo   python indexer.py "C:\path\to\your\project"
echo.
echo Example:
echo   python indexer.py "%CD%"
echo   python indexer.py "C:\Users\purli\source\MyGame"
echo.
echo ============================================================
echo Step 3: Configure Claude CLI
echo ============================================================
echo.
echo Add this to your Claude config:
echo   File: %%APPDATA%%\Claude\claude_desktop_config.json
echo.
echo {
echo   "mcpServers": {
echo     "codebase-search": {
echo       "command": "python",
echo       "args": ["%CD%\rag_server.py"]
echo     }
echo   }
echo }
echo.
echo Then restart Claude CLI.
echo.
echo ============================================================
echo See RAG_SETUP.md for detailed instructions
echo ============================================================
pause
