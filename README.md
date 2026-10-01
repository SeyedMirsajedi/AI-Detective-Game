\# AI Detective Game



AI Detective Game is a 2D detective mystery project developed in Unity as a bachelor/final university project.



The player investigates a murder inside Bellamy House by exploring rooms, inspecting clues, searching containers, talking to suspects, and eventually making a final accusation.



A major focus of the project is free-form NPC dialogue powered by a hosted large language model while keeping the actual case logic deterministic and controlled by the game.



\## Project Overview



The game uses a top-down house map for room navigation.



Each room is presented as a separate 2D side-view scene where the player can:



\- move through the environment;

\- interact with NPCs;

\- inspect physical clues;

\- search containers;

\- review collected evidence;

\- progress through the investigation.



NPC conversations accept free-form player input.



Unity sends dialogue requests to an ASP.NET Core backend, which then communicates with an OpenAI-compatible hosted LLM API.



The LLM is responsible only for generating natural dialogue and emotional reactions.



Important game information such as case truth, clue state, NPC knowledge, room access, progression, and win conditions remains controlled by Unity and the backend.



\## Core Features



\- Six-room Bellamy House investigation

\- Side-view 2D room exploration

\- House-map room navigation

\- Physical clue inspection

\- Searchable containers

\- Evidence notebook

\- Free-form NPC conversations

\- Hosted AI dialogue integration

\- Controlled NPC knowledge and private facts

\- Persistent investigation state

\- Restricted-room progression

\- Final suspect accusation

\- CASE SOLVED / CASE FAILED resolution

\- Dossier-style investigation UI

\- Main Menu and Settings interface



\## Architecture



The dialogue architecture is intentionally modular.



```text

Unity

&#x20; |

&#x20; | HTTP / JSON

&#x20; v

ASP.NET Core Backend

&#x20; |

&#x20; | IDialogueProvider

&#x20; v

OpenAI-Compatible Dialogue Provider

&#x20; |

&#x20; v

Hosted LLM API

