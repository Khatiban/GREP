# Grep.NET – Grep-Like File Search Tool in .NET

A command-line utility built with C# and .NET that mimics the functionality of Unix's `grep`, with additional features like recursive search, result limiting, and cancellation support.

## 📚 Course Project

- **Student Name:** Ehsan Khatiban
- **Course:** Parallel and Ansynchronous Programming with .NET
- **Professor:** Dietrich Birngruber
- **Date:** 09.05.25

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
Main() → ParseArguments() → PerformSearchAsync() → DisplayResults() → RetryPrompt()```

# Code Structure Overview

- **Arguments** are parsed to determine search term, path, pattern, and flags.  
- `PerformSearchAsync` uses `Directory.GetFiles` and `File.ReadLines` to locate and parse files.  
- Search supports cancellation via `CancellationTokenSource`.  
- If no arguments are passed, it enters **interactive mode** for user input.  

---

## 🛠️ Key Code Snippets

### Argument Parsing  
```csharp
ParseArguments(args, ref searchTerm, ref filePattern, ...);
```  
- Supports: `-d:path`, `-r`, `-t:N`  

### Async Search Logic  
```csharp
await PerformSearchAsync(...);
```  
- Efficient file traversal and line scanning.  
- Checks for cancellation requests during the operation.  

### Interactive Mode  
```csharp
GetUserInput(...); // Prompts if no args are passed
```  

---

## 💻 Example Usage

### Command-Line Mode  
```bash
GREP.exe "hello world" -r -d:C:\Projects -t:50
```  

### Interactive Mode  
```plaintext
Enter search term: "error"
File pattern: *.log
Search recursively? (y/n): y
```  

### Sample Output  
```plaintext
C:\Projects\log1.log: [ERROR] Something bad happened  
C:\Projects\log2.log: [ERROR] Unable to connect  
```  