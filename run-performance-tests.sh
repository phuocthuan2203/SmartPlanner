#!/bin/bash

# SmartPlanner Performance Tests Runner
# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== SmartPlanner Performance Tests ===${NC}"
echo -e "${YELLOW}Starting performance test suite...${NC}"

# Create results directory
mkdir -p TestResults/Performance
mkdir -p BenchmarkResults

# Build solution first
echo -e "${YELLOW}Building solution...${NC}"
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo -e "${RED}Build failed. Exiting.${NC}"
    exit 1
fi

# Run NBomber load tests
echo -e "${YELLOW}Running NBomber load tests...${NC}"
dotnet test tests/SmartPlanner.Tests.Performance/ --configuration Release --logger "trx;LogFileName=performance-tests.trx" --results-directory TestResults/Performance
LOAD_TEST_RESULT=$?

# Run BenchmarkDotNet database performance tests
echo -e "${YELLOW}Running database performance benchmarks...${NC}"
cd tests/SmartPlanner.Tests.Performance/
dotnet run --configuration Release -- --filter "*DatabasePerformanceTests*"
BENCHMARK_RESULT=$?
cd ../../

# Move benchmark results
if [ -d "tests/SmartPlanner.Tests.Performance/BenchmarkDotNet.Artifacts" ]; then
    mv tests/SmartPlanner.Tests.Performance/BenchmarkDotNet.Artifacts/* BenchmarkResults/ 2>/dev/null || true
fi

# Generate summary report
echo -e "${YELLOW}Generating performance test report...${NC}"
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
cat > TestResults/Performance/performance-summary.md << EOF
# Performance Test Summary
**Generated:** $TIMESTAMP

## Load Test Results
EOF

if [ $LOAD_TEST_RESULT -eq 0 ]; then
    echo -e "✅ **Load tests:** PASSED" >> TestResults/Performance/performance-summary.md
    echo -e "${GREEN}✅ Load tests: PASSED${NC}"
else
    echo -e "❌ **Load tests:** FAILED" >> TestResults/Performance/performance-summary.md
    echo -e "${RED}❌ Load tests: FAILED${NC}"
fi

cat >> TestResults/Performance/performance-summary.md << EOF

## Database Benchmark Results
EOF

if [ $BENCHMARK_RESULT -eq 0 ]; then
    echo -e "✅ **Database benchmarks:** COMPLETED" >> TestResults/Performance/performance-summary.md
    echo -e "${GREEN}✅ Database benchmarks: COMPLETED${NC}"
else
    echo -e "❌ **Database benchmarks:** FAILED" >> TestResults/Performance/performance-summary.md
    echo -e "${RED}❌ Database benchmarks: FAILED${NC}"
fi

# List generated files
echo -e "\n${BLUE}Generated Files:${NC}"
find TestResults/Performance -name "*.trx" -o -name "*.md" | while read file; do
    echo -e "${GREEN}- $file${NC}"
done

find BenchmarkResults -name "*.html" -o -name "*.json" 2>/dev/null | while read file; do
    echo -e "${GREEN}- $file${NC}"
done

# Final status
if [ $LOAD_TEST_RESULT -eq 0 ] && [ $BENCHMARK_RESULT -eq 0 ]; then
    echo -e "\n${GREEN}🎉 All performance tests completed successfully!${NC}"
    exit 0
else
    echo -e "\n${YELLOW}⚠️  Some performance tests had issues. Check the results above.${NC}"
    exit 1
fi
