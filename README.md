# MyChat (personal project)
This project began as an initiative to learn how to use OpenAI's GPT API library. I developed it as a Windows Presentation Foundation (WPF) 
desktop application. As this is my first experience with WPF, there are numerous opportunities for improvement. I am actively using this 
application as an alternative to ChatGPT

## Features
The desktop application functions similarly to ChatGPT but lacks many of its advanced features. Below is a list of some of its capabilities:

* Supports both the 4o and 4o-mini models.
* Allows customization of conversations by tone (such as professional, casual, techcnical, etc.), instructions, and topic.
* Provides the option to create conversations using templates, which are predefined combinations of tone, instructions, and topics.
* Offers a quick Q&A interface for individual questions.
* Supports image generation using DALL-E 3.
* The assistant can set the title of a conversation and the prompt in the image tool upon request.

## Roadmap
Although I am not currently developing this project further, there are several aspects I would like to improve. A future web version of this app will address some of these.

* Desktop application limitations: The primary issue with being a desktop application is its lack of portability; it requires being at my desk to use it. Development of a web version is underway.
* Flat File Usage: Conversations are saved as serialized JSON files, which necessitate manual saving to prevent data loss.
* Assistant Behavior: The GPT assistant sometimes behaves unpredictably, similar to a toddler, by executing tool commands unexpectedly despite explicit instructions. Finding a solution to this is challenging.
* Q&A Feature: This feature is not functioning as intended. I am considering redesigning it for working with the new o1 models.
* Topic Feature: I am dissatisfied with the current functionality of the topic feature and am contemplating changinging its semantics to serve as additional notes instead.
* GPT Model: Model selection is at a global level. I am considering making it a per conversation option like tone.

## Technical Details
This project was developed using Visual Studio and .Net 8.0 with C#. The following technologies were used in its creation:

* WPF using the MVVM pattern (with MVVM Community Toolkit)
* Dependency injection (with Microsoft.Extentions.DependencyInjection)
* WebView2
* Markdig (library for translating markdown to HTML)
* OpenAI (.Net library for accessing GPT)
* Highlight.js (additional HTML formatting)

