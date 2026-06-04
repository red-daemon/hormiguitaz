#!/bin/bash

# Ant Simulator - Automated Commit Script
# Usage: ./scripts/commit.sh "feat: description"

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_step() {
    echo -e "${GREEN}▶${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_success() {
    echo -e "${GREEN}✅${NC} $1"
}

# Check if commit message is provided
if [ -z "$1" ]; then
    print_error "No commit message provided"
    echo "Usage: ./scripts/commit.sh \"<type>: <description>\""
    echo "Types: feat, fix, docs, refactor, test, chore, debug, perf"
    exit 1
fi

COMMIT_MSG="$1"

# Validate conventional commit format
VALID_TYPES="feat|fix|docs|refactor|test|chore|debug|perf"
if ! [[ "$COMMIT_MSG" =~ ^($VALID_TYPES):[[:space:]] ]]; then
    print_error "Invalid commit message format"
    echo "Must start with: feat:, fix:, docs:, refactor:, test:, chore:, debug:, or perf:"
    exit 1
fi

print_step "Starting commit workflow..."
echo ""

# Step 1: Format code
print_step "Running dotnet format..."
if dotnet format ant-simulator.slnx; then
    print_success "Code formatting complete"
else
    print_error "dotnet format failed"
    exit 1
fi
echo ""

# Step 2: Build with analyzers (Release mode)
print_step "Building project (Release mode with analyzers)..."
if dotnet build ant-simulator.slnx -c Release --no-restore 2>&1 | tee /tmp/build.log; then
    if grep -q "warning:" /tmp/build.log; then
        print_warning "Build completed with warnings"
    else
        print_success "Build successful"
    fi
else
    print_error "Build failed"
    exit 1
fi
echo ""

# Step 3: Run tests with coverage
print_step "Running tests with code coverage..."
TEST_RESULTS_DIR="./TestResults"
rm -rf "$TEST_RESULTS_DIR"

if dotnet test --collect:"XPlat Code Coverage" --results-directory "$TEST_RESULTS_DIR" --no-build -c Release; then
    print_success "All tests passed"
else
    print_error "Tests failed"
    exit 1
fi
echo ""

# Step 4: Parse and check code coverage
print_step "Checking code coverage..."
COVERAGE_FILE=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" | head -1)

if [ -z "$COVERAGE_FILE" ]; then
    print_warning "Could not find coverage report"
else
    # Extract line-rate from coverage XML
    LINE_RATE=$(grep -oP 'line-rate="\K[^"]+' "$COVERAGE_FILE" | head -1)
    if [ -n "$LINE_RATE" ]; then
        COVERAGE_PERCENT=$(awk "BEGIN {printf \"%.1f\", $LINE_RATE * 100}")
        echo "Code coverage: ${COVERAGE_PERCENT}%"

        # Check if coverage meets minimum threshold (70%)
        if (( $(echo "$COVERAGE_PERCENT < 70" | bc -l) )); then
            print_error "Code coverage below 70% threshold (${COVERAGE_PERCENT}%)"
            exit 1
        fi
        print_success "Code coverage acceptable (${COVERAGE_PERCENT}%)"
    fi
fi
echo ""

# Step 5: Stage changes
print_step "Staging changes..."
git add -A
print_success "Changes staged"
echo ""

# Step 6: Create commit
print_step "Creating commit..."
if git commit -m "$(cat <<EOF
$COMMIT_MSG

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>
EOF
)"; then
    print_success "Commit created"
else
    print_error "Commit failed"
    exit 1
fi
echo ""

# Step 7: Push to remote
print_step "Pushing to origin..."
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
if git push origin "$CURRENT_BRANCH"; then
    print_success "Pushed to origin/$CURRENT_BRANCH"
else
    print_error "Push failed"
    exit 1
fi
echo ""

print_success "Commit workflow completed successfully!"
