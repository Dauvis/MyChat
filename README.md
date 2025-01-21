# MyChat
The primary goal of this learning project was to explore the GPT API using OpenAI's .NET library. I developed it using Windows Presentation Foundation (WPF) to gain insights into that platform. To be honest, I am not entirely satisfied with the final result; I wouldn’t consider it a finished product.

## Features
The desktop application operates similarly to ChatGPT but lacks many of its advanced features. Below is a list of its capabilities:

* Supports both the 4o and 4o-mini models.
* Allows customization of conversations by tone (such as professional, casual, technical, etc.), instructions, and topics.
* Provides the option to create conversations using templates, which are predefined combinations of tone, instructions, and topics.
* Offers a quick Q&A interface for answering individual questions.
* Supports image generation using DALL-E 3.
* The assistant can set the title of a conversation and the prompt in the image tool upon request.

## Technology
This project was developed using Visual Studio and .NET 8.0 with C#. I employed a pseudo N-tier architecture to implement various functionality layers. The following technologies were utilized in its creation:

* WPF with the MVVM pattern (using MVVM Community Toolkit)
* Dependency injection (via Microsoft.Extensions.DependencyInjection)
* WebView2
* Markdig (a library for converting markdown to HTML)
* OpenAI (.NET library for accessing GPT)
* Highlight.js (for additional HTML formatting)

## Future
While I am not continuing development on this project, several aspects need improvement. These issues will be addressed in the web version.

* Limitations of the desktop application: The main drawback of being a desktop application is its lack of portability; it can only be used while at my desk.
* Flat file usage: Conversations are saved as serialized JSON files, which require manual saving to prevent data loss.
* Assistant behavior: The GPT assistant sometimes behaves unpredictably, executing tool commands unexpectedly, much like a toddler, even when given explicit instructions. I believe the lesson here is that when providing tools, the assistant needs to remain focused on the tasks that utilize them.
* Q&A feature: This feature is not functioning as intended. I am considering redesigning it to work with the new o1 models.
* Topic feature: I am dissatisfied with the current functionality of the topic feature and am contemplating changing its purpose to serve as additional notes instead.
* GPT model: Model selection is currently at a global level. I am considering making it a per-conversation option, similar to tone.
