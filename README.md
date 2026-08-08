# MCP Server for LubeLogger

<img width="735" height="306" alt="image" src="https://github.com/user-attachments/assets/85887c27-48aa-42ff-b9db-f5a3fe225a9a" />

This is a technical tool mainly for experimentation and exploration into AI-enabled features in LubeLogger. 

AI and MCP are still evolving technologies, implementations of this project is subject to break without prior notice.

## Pre-requisites
- AI agent with ability to call tools/external integrations(i.e.: Claude Desktop)
- Node.js(for npx)

## Getting Started

This MCP Server allows for authentication to be configured at the AI-Agent level(Header Auth) or at the MCP Server level(Local Auth).

Header Auth is more complex to set up but offers greater security, whereas Local Auth is easier to set up but allows users unfettered access to all vehicles in the instance.

For details on both configurations, see [Wiki](https://github.com/hargata/lubelog_mcp/wiki)

## Commands Supported

- Retrieve list of vehicles
- Add Vehicle from VIN
- Add Fuel Record from image of receipt
- Add Service/Repair/Upgrade records from invoice
- Add Odometer record from image of dashboard
- Add Supply(or shop supply) record from image of invoice
- Get latest odometer reading from vehicle
- Get Fuel/Service/Repair/Upgrade/Odometer/Supply records for a vehicle
- Get shop supply records
- Check status of LubeLogger instance

## Example Usage

<img width="760" height="700" alt="image" src="https://github.com/user-attachments/assets/a99f4570-adae-406c-914c-d580fcf3cce6" />

<img width="758" height="566" alt="image" src="https://github.com/user-attachments/assets/abf9ef37-7cfe-4701-b1b7-272e8a474841" />

<img width="735" height="561" alt="image" src="https://github.com/user-attachments/assets/a032512f-66c4-4a0a-8f98-54f7d28a623a" />
