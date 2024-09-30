# Introduction

This is a C# based resiliency wrapper around Polly.Net

The package provides useful helpers for handling async and synchronous code retries

Demonstrates the following
- Packaging to Nuget
- Azure Pipelines
- NUnit tests

# Usage

Build and update the feed details on the 'Push to Nuget' in `deploy.yaml`

Then consume by either calling one of supported IoC provider extension methods or roll your own using the IResilienceRetry

Refer to the documentation comments in the code
