# Project Rules

## Locked files / busy ports
- If a build or run fails because a file is locked by another process (e.g. MSB3021/MSB3027 on `TrainBooking.Api.exe`) or a port is already in use (e.g. 7777), STOP immediately and notify the user.
- Do NOT work around it: no building to a temporary/scratch output directory, no running a parallel copy on another port, and no killing the user's processes.
- Wait for the user to free the file/port before retrying.

## Architecture (intentional, do not change)
- This project uses DDD with three layers: Domain, Application, Infrastructure.
- Layer separation and dependency direction are deliberate.
- Do NOT suggest merging layers, removing repository/domain interfaces, or
  collapsing entities, value objects and handlers to reduce file count.
- Architecture review suggestions must work within this structure. Flag
  anything that would break it instead of proposing it.
