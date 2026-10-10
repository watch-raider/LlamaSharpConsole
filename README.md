# LlamaSharpConsole

## Purpose

LlamaSharpConsole is a personal exploration project for evaluating the [LlamaSharp](https://github.com/SciSharp/LLamaSharp) library as a performant alternative for running local LLM inference in .NET.

## Motivation

This project was created to assess performance improvements over existing Python-based solutions. Previously, I was working with Python using Ollama and Langchain for local model inference, but experienced disappointing performance results. This project explores whether leveraging LlamaSharp in a .NET environment can deliver better performance characteristics for running small local models.

## What It Does

LlamaSharpConsole provides a console-based interface for:
- Running inference with small local language models (e.g., Ministral-3-3B)
- Managing chat sessions and conversation history
- Executing batch operations through a session executor
- Persisting state across sessions for stateful interactions

## Technology Stack

- **.NET 10.0** - Runtime framework
- **LlamaSharp** - LLM inference library
- **Local GGUF Models** - Quantized model format for efficient local execution

## Project Structure

- `Program.cs` - Application entry point
- `model/ChatSession.cs` - Chat session management
- `models/` - Local model files (GGUF format)
- `session_state/` - Persistent session state and chat history
- `session_executor/` - Batch execution state
