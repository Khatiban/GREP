# Grep.NET – Grep-Like File Search Tool in .NET

A command-line utility built with C# and .NET that mimics the functionality of Unix's `grep`, with additional features like recursive search, result limiting, and cancellation support.

## 📚 Course Project

- **Student Name:** [Insert Your Name]  
- **Course:** [Insert Course Name, e.g., Advanced C# Programming]  
- **Professor:** [Insert Professor’s Name]  
- **Date:** [Insert Date]

---

## 🚀 Key Features

- **Recursive File Search:** Use `-r` flag to search through subdirectories.
- **Result Limit:** Use `-t:N` to limit the number of matches (e.g., `-t:100`).
- **Cancellation Support:** Press `c` during search to cancel operation.
- **Async Processing:** Uses `Task.Run` and asynchronous file reading for responsiveness.

---

## 🧠 How It Works

### Application Flow

```plaintext
Main() → ParseArguments() → PerformSearchAsync() → DisplayResults() → RetryPrompt()
