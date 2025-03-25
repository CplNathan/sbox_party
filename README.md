# sbox Party!
sbox Party! is a multiplayer board game for s&box, inspired by party games like Mario Party and Pummel Party. Players move around a game board, competing for coins and rewards that advance them to victory. Between turns, they play mini-games to earn power-ups and hinder other players' progress.

The goal of this project is to leverage the powerful tools and interfaces behind s&box (e.g, leaderboards, lobbies, and other quality-of-life features) to create a seamless and engaging experience. Thanks to Facepunch's detailed documentation and source code, I’ve gained a strong understanding of the engine and tooling, forming a solid foundation for this project.

## Progress
- **Networked Gameplay:**
  - The game currently supports networked play, where players can spawn on the initial game board, take turns rolling physically simulated dice, and interact with them (e.g, grab and throw them before tossing them).
  - The turns and dice mechanics are fully networked, secured via verified RPCs, and compatible with dedicated servers.

- **Minigame Integration:**
  - After each player takes a turn, players are loaded into an empty minigame. The game is ready for additional boards and minigames, with this expansion being relatively straightforward thanks to the tools provided by s&box.

- **Visual and Design Potential:**
  - While the game is functional, there’s significant potential to enhance the visual quality, particularly through shaders and level design. My experience in set dressing and level design is currently limited, but I'm working on refining these aspects to create a better visual experience.

- **Challenges with Time and Design:**
  - One of the main challenges I’m facing is balancing coding with visual/level design, as I don’t have professional experience in the latter. As I focus on technical development, I’ve realized the need for a second pair of hands to handle the level design and texturing. This would allow me to concentrate on improving the game’s mechanics and expanding its features without getting bogged down by the visual details.

### Custom Editor Features
- **Custom Assets/Resources** for defining boards and minigames within the editor explorer
![Game Resource](https://github.com/user-attachments/assets/6a70c2c5-204a-4fe8-ac6d-c2af3c957398)

- **Cubemap Generator** for interior cubemap shader graph
![Cubemap Tool](https://github.com/user-attachments/assets/2d56c1a4-dd45-4555-b460-b22b2ea6821f)

- **Cool Shaders**
- - Interior Cube Mapping
![Interior Shader](https://github.com/user-attachments/assets/4fdfaa43-b99f-4bb1-b3d2-bb27629bf918)

- - Height Blended Parallaxed Mud/Grass
![Ground Shader](https://github.com/user-attachments/assets/00942c71-c4f9-4426-9814-7d2bc4510589)


### Additional Photos
![Default Camera](https://github.com/user-attachments/assets/ecfe1a0a-1b2b-43ef-8991-358566b555be)
