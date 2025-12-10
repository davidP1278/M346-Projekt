#!/usr/bin/env bash
set -e

REGION="us-east-1"

RANDOM_ID=$(date +%s)
IN_BUCKET_NAME="yvauda-input-${RANDOM_ID}"
OUT_BUCKET_NAME="yvauda-output-${RANDOM_ID}"
LAMBDA_FUNCTION_NAME="yvaudalambdafunction"


aws s3api create-bucket \
  --bucket "$IN_BUCKET_NAME" \
  --region "$REGION"

aws s3api create-bucket \
  --bucket "$OUT_BUCKET_NAME" \
  --region "$REGION"

echo "buckets generiert"




aws lambda update-function-configuration \
  --function-name "$LAMBDA_FUNCTION_NAME" \
  --environment "Variables={OUT_BUCKET=$OUT_BUCKET_NAME}"

echo "lambda konfiguriert"
