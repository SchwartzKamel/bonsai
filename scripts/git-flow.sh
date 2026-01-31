# git-flow script for conventional commits using GitHub CLI (gh)

# Usage: ./scripts/git-flow.sh [add|commit|push] [options]

#!/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(git rev-parse --show-toplevel)"

# Ensure gh is installed and authenticated
if ! command -v gh && ! gh auth status >/dev/null 2>&1; then
  echo "Error: GitHub CLI (gh) is required and must be authenticated."
  exit 1
fi

case "$1" in
  add)
    git add .
    echo "Staged all changes."
    ;;
  commit)
    if [ -z "$2" ]; then
      echo "Usage: $0 commit \"<conventional commit message>\""
      echo "Example: $0 commit \"feat: add new feature\""
      exit 1
    fi
    git commit -m "$2"
    echo "Committed with message: $2"
    ;;
  push)
    if [ "$(git rev-parse --abbrev-ref HEAD)" = "main" ]; then
      git push origin main
    else
      git push origin HEAD
    fi
    echo "Pushed to origin $(git rev-parse --abbrev-ref HEAD)."
    ;;
  *)
    echo "Usage: $0 {add|commit \"message\"|push}"
    echo "Follows conventional commits: type(scope): subject"
    exit 1
    ;;
esac