# Oura export fixtures

`oura-sandbox-exports.json` contains synthetic responses fetched from Oura's official `/v2/sandbox/usercollection/` endpoints on 22 October 2026 using the documented arbitrary sandbox authorization header. No real account credentials or user data were used. `personal_info` is a local synthetic fixture because that endpoint has no sandbox route.

Sandbox templates sometimes retain example dates instead of matching the query; fixture tests select the record's own day when validating field extraction. Timestamp values are parsed as strings so the UTC offset is retained.

`oura-export-schemas.json` is the relevant model/ref subset of Oura's official `https://cloud.ouraring.com/v2/static/json/openapi-1.42.json`, loaded by the current V2 docs. The field-path regression tests resolve catalog paths against those models. These fixtures are deliberately checked in for reproducible tests without an Oura account or network access.
