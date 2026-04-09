# Backend CI Workflow

## Purpose
This document explains the GitHub Actions CI workflow added for the ZyntraPay backend.

## What It Does

The workflow automatically runs on:

- push to `main`
- push to `develop`
- push to `feature/**`
- pull request to `main`
- pull request to `develop`
- manual trigger through GitHub Actions

It performs these steps:

1. Checkout repository
2. Setup .NET 8 SDK
3. Restore backend solution
4. Build backend solution
5. Run backend tests

## Workflow File

- [backend-ci.yml](e:\DotNet_Learning\ZyntraPay\.github\workflows\backend-ci.yml)

## Why It Matters

This helps the case study because:

- backend quality is validated automatically
- build issues are caught before merge
- test failures are visible on GitHub
- it demonstrates modern team workflow and DevOps awareness

## Scope

This workflow currently covers backend only.

It uses:

- solution file: [ZyntraPay.slnx](e:\DotNet_Learning\ZyntraPay\Backend\ZyntraPay.slnx)
- working directory: `Backend/`

## Current Design Choice

The workflow is intentionally minimal:

- no deployment step yet
- no Docker image publishing yet
- no coverage upload yet

This keeps it simple, stable, and suitable for a case study.

## Possible Future Extensions

Later, if needed, this workflow can be extended with:

- Docker build validation
- test coverage artifact upload
- frontend CI workflow
- deployment pipeline
