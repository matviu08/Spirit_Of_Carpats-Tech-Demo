# Spirit_Of_Carpats_Remake
Повний список систем гри 
1. Система гравця 
• рух 
• стрибок 
• взаємодія 
• стан гравця 
2. Фізична система 
• гравітація 
• колізії 
• платформи 
• тригери 
3. Система анімацій 
• анімації персонажа 
• анімації об'єктів 
• перемикання анімацій (state machine) 
4. Система освітлення 
• джерела світла 
• тіні 
• ліхтар гравця 
• затемнення сцени 
5. Система рендерингу 
• відображення спрайтів 
• відображення тайлів 
• порядок рендерингу 
6. Система локацій 
• tilemap 
• завантаження рівнів 
• межі карти 
7. Система предметів 
• створення предметів 
• типи предметів 
• опис предметів 
8. Інвентар 
• зберігання предметів 
• використання предметів 
• видалення предметів 
9. Система взаємодії 
• взаємодія з об'єктами 
• двері 
• записки 
• важелі 
10. Система сюжету 
• текстові події 
• сюжетні тригери 
• показ записок 
11. Система камери 
• слідування за гравцем 
• обмеження камери 
• плавний рух 
Додаткові важливі системи 
12. Система збереження гри 
• збереження прогресу 
• збереження інвентаря 
• збереження позиції гравця 
13. Система сцен 
• переключення рівнів 
• завантаження сцен 
• управління сценою 
14. Меню гри 
• головне меню 
• меню паузи 
• меню налаштувань 
15. Аудіосистема 
• фонові звуки 
• звуки кроків 
• атмосферні звуки 
• звуки предметів 
16. Event System (система подій) 
• система тригерів 
• виклик подій 
• реакції систем на події 
Розподіл між програмістами 
Програміст 1 — Gameplay 
Відповідає за ігрову логіку. 
Системи 
1. Система гравця 
2. Система предметів 
3. Інвентар 
4. Система взаємодії 
5. Система сюжету 
6. Event System 
Програміст 2 — Світ і фізика 
Відповідає за поведінку об'єктів у світі. 
Системи 
1. Фізична система 
2. Система камери 
3. Система локацій (tilemap) 
4. Система сцен 
5. Система збереження 
Програміст 3 — Графіка та атмосфера 
Відповідає за візуальну частину гри. 
Системи 
1. Рендеринг 
2. Система анімацій 
3. Система освітлення 
4. UI 
5. Меню гри 
6. Аудіосистема 
Загальна архітектура 
Приклад структури проєкту: 
Game 
│ 
├── Core 
│   ├── GameLoop 
│   ├── SceneManager 
│   ├── EventSystem 
│ 
├── Physics 
│   ├── CollisionSystem 
│   ├── Rigidbody 
│ 
├── Gameplay 
│   ├── Player 
│   ├── Inventory 
│   ├── Items 
│   ├── Interaction 
│ 
├── World 
│   ├── Tilemap 
│   ├── LevelLoader 
│ 
├── Graphics 
│   ├── Renderer 
│   ├── Animation 
│   ├── Lighting 
│ 
├── Audio 
│ 
├── UI 
│ 
└── Save 
Порядок розробки (дуже важливо) 
Етап 1 — база гри 
• рендеринг 
• фізика 
• рух персонажа 
• камера 
Етап 2 — світ гри 
• карта 
• сцени 
• взаємодія 
Етап 3 — геймплей 
• предмети 
• інвентар 
• сюжет 
Етап 4 — атмосфера 
• освітлення 
• анімації 
• аудіо 
• меню 
Тех-частина і документації 
Програміст 1 — Gameplay (геймплей) 
Його системи 
• гравець 
• інвентар 
• предмети 
• взаємодія 
• сюжет 
• тригери подій 
Документація яку він використовує 
Ввід (керування) 
https://www.raylib.com/examples/core/core_input_keys.html 
Функції: 
IsKeyDown() 
IsKeyPressed() 
GetKeyPressed() 
Робота з текстом (записки, сюжет) 
https://www.raylib.com/examples/text/text_writing_anim.html 
Функції: 
DrawText() 
MeasureText() 
LoadFont() 
Робота з прямокутниками (взаємодія) 
CheckCollisionRecs() 
CheckCollisionPointRec() 
Структури 
Vector2 
Rectangle 
Що він реалізує 
PlayerController 
InventorySystem 
ItemSystem 
InteractionSystem 
StorySystem 
Програміст 2 — Physics + World 
Його системи 
• фізика 
• колізії 
• карта 
• камера 
• рівні 
• збереження 
Документація 
Камера 2D 
https://www.raylib.com/examples/core/core_2d_camera.html 
Тип: 
Camera2D 
Поля: 
target 
offset 
zoom 
rotation 
Колізії 
https://www.raylib.com/examples/shapes/shapes_collision_area.html 
Функції: 
CheckCollisionRecs() 
CheckCollisionCircles() 
Фізика (основи) 
Vector2 
GetFrameTime() 
Delta time: 
f
loat dt = GetFrameTime(); 
position += velocity * dt; 
Tilemap / карта 
https://www.raylib.com/examples/textures/textures_tiled_map.html 
Функції: 
LoadTexture() 
DrawTexture() 
DrawTextureRec() 
Що він реалізує 
PhysicsSystem 
CollisionSystem 
PlayerMovementCameraSystem 
TilemapSystem 
LevelLoader 
Програміст 3 — Graphics + Lighting 
Його системи 
• рендеринг 
• анімації 
• освітлення 
• UI 
• звук 
Документація 
Робота з текстурами 
https://www.raylib.com/examples/textures/textures_sprite_anim.html 
Функції: 
LoadTexture() 
DrawTexture() 
DrawTextureRec() 
UnloadTexture() 
Спрайт анімації 
DrawTextureRec() 
Rectangle frameRec 
Шейдери (світло) 
https://www.raylib.com/examples/shaders/shaders_basic_lighting.html 
Функції: 
LoadShader() 
BeginShaderMode() 
EndShaderMode() 
Малювання 
BeginDrawing() 
EndDrawing() 
ClearBackground() 
Звук 
https://www.raylib.com/examples/audio/audio_music_stream.html 
Функції: 
InitAudioDevice() 
LoadSound() 
PlaySound() 
Що він реалізує 
RenderSystem 
AnimationSystem 
LightingSystem 
UISystem 
AudioSystem 
Фінальний розподіл 
Програміст Системи 
1 Gameplay Player, Items, Inventory, Interaction, Story 
2 Physics 
Physics, Collision, Camera, Tilemap, Levels 
3 Graphics Rendering, Animation, Lighting, UI, Audio 
Найважливіші приклади raylib (обов'язково) 
Всім подивитися: 
https://www.raylib.com/examples/core/core_basic_window.html 
https://www.raylib.com/examples/core/core_2d_camera.html 
https://www.raylib.com/examples/textures/textures_sprite_anim.html 
https://www.raylib.com/examples/shaders/shaders_basic_lighting.html 