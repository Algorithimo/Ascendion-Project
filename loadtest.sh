#!/bin/bash
echo "============================================"
echo " Hacker News API - Load Test (Bombardier)"
echo "============================================"
echo ""
API_URL="${1:-http://localhost:5123}"

echo "--- Warm-up (5 requests) ---"
bombardier -n 5 -c 1 "$API_URL/api/stories?count=10" 2>/dev/null
echo ""

echo "--- Test 1: Throughput (100 connections, 10s) ---"
bombardier -c 100 -d 10s --print result "$API_URL/api/stories?count=10"
echo ""

echo "--- Test 2: Latency under load (50 connections, 10s) ---"
bombardier -c 50 -d 10s --latencies --print result "$API_URL/api/stories?count=10"
echo ""

echo "--- Test 3: Heavy payload (count=100, 25 connections, 10s) ---"
bombardier -c 25 -d 10s --latencies --print result "$API_URL/api/stories?count=100"
echo ""

echo "============================================"
echo " Done! Review latency p50/p95/p99 above."
echo "============================================"
