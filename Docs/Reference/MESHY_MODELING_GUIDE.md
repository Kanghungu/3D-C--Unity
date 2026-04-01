# MESHY_MODELING_GUIDE.md

## 이 문서의 목적
- 이 문서는 Meshy에 바로 복사해서 붙여넣을 수 있는 `실전용 프롬프트 시트`다.
- 설명보다 `바로 실행 가능한 설정 + 프롬프트`를 우선한다.
- 이 프로젝트의 핵심 스타일은 `중장갑 성전 제국` 대 `정면 화력형 문명`이다.

## 문서 읽는 법
- 바로 하나 만들어보고 싶으면:
  - `1. 바로 복붙 프롬프트`
  - `2. 짧은 작업 순서`
- Meshy 기능 역할부터 이해하고 싶으면:
  - `4. Meshy 기능 설명`
  - `5. 기능 역할 요약`
- 리메시 값을 빨리 정하고 싶으면:
  - `5-1. 지금 보이는 Remesh 창 기준으로 고르는 법`
  - `5-6. 스크린샷 같은 인간형 창병에는 무엇을 고르면 되나`
  - `5-7. 병기형에는 무엇을 고르면 되나`
- 모델별로 아주 자세히 따라가고 싶으면:
  - `3. 상세 제작 매뉴얼`

## 빠른 시작
1. 만들 유닛 하나를 고른다.
2. 아래 프롬프트를 그대로 복붙한다.
3. `4 generations`로 4개를 본다.
4. 가장 좋은 1개를 고른다.
5. `Remesh -> Texture -> Texture Edit` 순으로 정리한다.
6. 인간형이면 `Rigging -> Animate`까지 간다.
7. `FBX`로 내보내고 Unity에서 확인한다.

## 0. 유닛별 통합 작업 카드

이 섹션은 실제 작업할 때 `위아래로 계속 스크롤하지 않도록` 만든 빠른 작업 카드다.
각 유닛마다 `생성 설정 -> 프롬프트 -> Remesh -> Texture -> Rigging/Animate -> Export`를 한 자리에서 보게 정리했다.
집에서 작업할 때는 이 섹션만 보고 바로 진행해도 된다.

### 0-1. 플레이어 창병 작업 카드
- 역할: 플레이어 진영 기본 근접 보병
- 생성 설정: `Meshy 6`, `Standard`, `T-Pose`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `창이 분명하고`, `전신이 안 잘리고`, `어깨와 하체 비율이 안정적인 결과`
- 프롬프트:
```text
full body sci-fi crusader spearman, T-pose, symmetrical presentation, tall heavy infantry silhouette, long powered shock spear with blade tip held as part of the full model, broad plated shoulders, reinforced greaves, cathedral-inspired hard-surface armor, ivory ceramic plating, brushed brass trim, deep crimson cloth tabard, grimdark sci-fi crusader empire, noble but brutal military silhouette, clean hard-surface surfaces, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 10K + 사각형 면`
- Remesh 올리는 기준: 창, 손, 망토, 어깨가 무너지면 `고정형 + 30K + 사각형 면`
- Texture 추천 문장:
```text
ivory ceramic armor, brushed brass trim, deep crimson cloth accents, clean but battle-worn military finish, subtle edge wear, grimdark sci-fi crusader empire
```
- Rigging: `사용`
- Animate 최소 세트: `Idle`, `Walk`, `Run`, `Attack`, `Death`
- Export: 기본 `FBX`, 빠른 확인용만 `GLB`
- Unity 메모: 기본 보병 기준 유닛이라 `Move`와 `Attack` 전환이 자연스러운지 먼저 본다.

### 0-2. 플레이어 방패 보병 작업 카드
- 역할: 전선 유지용 중장 방어 보병
- 생성 설정: `Meshy 6`, `Standard`, `T-Pose`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `방패가 크게 읽히고`, `몸이 너무 가늘지 않고`, `방패와 무기가 빠지지 않은 결과`
- 프롬프트:
```text
full body sci-fi crusader shield infantry, T-pose, symmetrical presentation, broad defensive silhouette, massive rectangular energy shield mounted on one arm, compact sidearm holstered on the hip, layered chest armor, thick forearm guards, cathedral-inspired hard-surface armor, ivory ceramic armor, brushed brass trim, crimson cloth accents, grimdark sci-fi crusader empire, frontline defensive heavy infantry, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 10K + 사각형 면`
- Remesh 올리는 기준: 방패 가장자리와 팔 비율이 무너지면 `고정형 + 30K + 사각형 면`
- Texture 추천 문장:
```text
ivory ceramic armor, brushed brass trim, deep crimson cloth accents, reinforced shield plating, clean but battle-worn military finish, grimdark sci-fi crusader empire
```
- Rigging: `사용`
- Animate 최소 세트: `Idle`, `Walk`, `Brace`, `Attack`, `Death`
- Export: 기본 `FBX`
- Unity 메모: 이동보다 `버티는 느낌`이 중요해서 속도를 낮게 잡는다.

### 0-3. 적 총병 작업 카드
- 역할: 적 진영 기본 원거리 보병
- 생성 설정: `Meshy 6`, `Standard`, `T-Pose`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `총이 길고`, `가슴 장비와 어깨선이 각지고`, `적 진영처럼 차갑고 산업적으로 보이는 결과`
- 프롬프트:
```text
full body sci-fi rifle infantry, T-pose, symmetrical presentation, lean aggressive firing-line silhouette, long industrial pulse rifle held as part of the full model, angular armored chest rig, compact backpack power unit, dark gunmetal armor, burnt orange emissive accents, exposed weapon systems, frontline firepower civilization, harsh militarized engineering, aggressive battlefield silhouette, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 10K + 사각형 면`
- Remesh 올리는 기준: 총과 손, 어깨 구조가 무너지면 `고정형 + 30K + 사각형 면`
- Texture 추천 문장:
```text
dark gunmetal armor, burnt orange emissive accents, industrial military metal panels, heat-stained weapon surfaces, harsh utilitarian battlefield finish
```
- Rigging: `사용`
- Animate 최소 세트: `Idle`, `Walk`, `Run`, `Shoot`, `Death`
- Export: 기본 `FBX`
- Unity 메모: `Move -> Shoot` 전환이 어색하지 않은지 먼저 확인한다.

### 0-4. 적 포병 작업 카드
- 역할: 적 진영 중화기 병기
- 생성 설정: `Meshy 6`, `Standard`, `None`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `포신이 길고`, `받침 구조가 안정적이고`, `한눈에 포대처럼 보이는 결과`
- 프롬프트:
```text
sci-fi field artillery platform, low wide silhouette, oversized long-barrel siege cannon, reinforced recoil housing, heavy industrial armor plates, stabilizer legs, exposed loading mechanism, dark gunmetal body, burnt orange warning lights, frontline firepower civilization, harsh militarized engineering, brutal battlefield machine, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, no crew, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 10K + 삼각형 면`
- Remesh 올리는 기준: 포신, 받침, 차체가 뭉개지면 `고정형 + 30K + 삼각형 면`
- Texture 추천 문장:
```text
dark gunmetal armor, burnt orange warning lights, industrial military metal panels, heat-stained cannon barrel, harsh utilitarian battlefield finish
```
- Rigging: `사용 안 함`
- Animate: `사용 안 함`, 대신 Unity에서 `포신 회전`, `반동`, `발사 흔들림` 구현
- Export: 기본 `FBX`
- Unity 메모: 차체 축과 포신 축이 코드 제어하기 쉽게 들어오는지 확인한다.

### 0-5. 플레이어 특수전사 작업 카드
- 역할: 플레이어 진영 고기동 엘리트 돌격 보병
- 생성 설정: `Meshy 6`, `Standard`, `T-Pose`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `쌍검이 분명하고`, `점프팩이 붙어 있고`, `너무 마르지 않은 결과`
- 프롬프트:
```text
full body sci-fi elite shock warrior, T-pose, symmetrical presentation, compact athletic assault silhouette, twin energy blades attached to both hands, reinforced jump pack, segmented armored torso, black under-armor with ivory ceramic plating and brushed brass trim, crimson cloth accents, grimdark sci-fi crusader empire, elite assault infantry, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 30K + 사각형 면`
- Remesh 내리는 기준: 너무 무거우면 `고정형 + 10K + 사각형 면`도 비교
- Texture 추천 문장:
```text
black under-armor, ivory ceramic armor plates, brushed brass trim, deep crimson cloth accents, elite but battle-worn military finish, grimdark sci-fi crusader empire
```
- Rigging: `사용`
- Animate 최소 세트: `Idle`, `Walk`, `Run`, `Attack`, `Death`
- Export: 기본 `FBX`
- Unity 메모: 빠른 돌격 유닛이라 `Run`과 `Attack` 속도감이 중요하다.

### 0-6. 플레이어 친위대 작업 카드
- 역할: 플레이어 진영 최상위 중장 엘리트 근접 보병
- 생성 설정: `Meshy 6`, `Standard`, `T-Pose`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `할버드가 선명하고`, `갑옷이 장중하며`, `실루엣이 고급스럽게 무거운 결과`
- 프롬프트:
```text
full body royal guard sci-fi crusader, T-pose, symmetrical presentation, noble heavy elite silhouette, ceremonial power halberd held as part of the full model, integrated side shield, ornate cathedral armor, polished ivory armor, rich brushed brass trim, deep crimson royal cloth, imperial guardian presence, grimdark sci-fi crusader empire, elite palace battlefield guard, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 30K + 사각형 면`
- Remesh 내리는 기준: 테스트용 경량화가 필요하면 `고정형 + 10K + 사각형 면`
- Texture 추천 문장:
```text
polished ivory armor, rich brushed brass trim, deep crimson royal cloth, elite ceremonial military finish, subtle battle wear, grimdark sci-fi crusader empire
```
- Rigging: `사용`
- Animate 최소 세트: `Idle`, `Walk`, `Attack`, `Hit`, `Death`
- Export: 기본 `FBX`
- Unity 메모: 일반 보병보다 느리지만 더 무겁고 위엄 있게 보여야 한다.

### 0-7. 플레이어 전투기 작업 카드
- 역할: 플레이어 진영 소형 공중 전투기
- 생성 설정: `Meshy 6`, `Standard`, `None`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `기수 방향이 분명하고`, `좌우 균형이 좋고`, `날개와 엔진이 잘 읽히는 결과`
- 프롬프트:
```text
sci-fi fighter aircraft, sharp forward-swept silhouette, twin engines, nose cannon, missile hardpoints, armored fuselage, cathedral-inspired military styling, ivory panels, brushed brass details, crimson markings, grimdark sci-fi crusader empire, fast readable combat aircraft, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, landing gear retracted, no base, no environment, no background, no text, no logo, no smoke trails, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 10K + 삼각형 면`
- Remesh 올리는 기준: 날개 끝, 엔진부, 기수 디테일이 무너지면 `고정형 + 30K + 삼각형 면`
- Texture 추천 문장:
```text
ivory military panels, brushed brass details, crimson markings, clean hard-surface aircraft finish, subtle battle wear, grimdark sci-fi crusader empire
```
- Rigging: `사용 안 함`
- Animate: `사용 안 함`, 대신 Unity에서 `전진`, `뱅킹`, `상승/하강`, `발사` 구현
- Export: 기본 `FBX`
- Unity 메모: 기수 방향과 로컬 축이 예상대로 들어오는지 먼저 확인한다.

### 0-8. 플레이어 이동 거점 작업 카드
- 역할: 플레이어 진영 지상 이동 요새
- 생성 설정: `Meshy 6`, `Standard`, `None`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `거대한 차체`, `포탑 구조`, `이동 성채처럼 보이는 실루엣`
- 프롬프트:
```text
mobile fortress vehicle, colossal land battleship silhouette, layered armor citadel, multiple heavy turrets, cathedral-inspired command tower, massive tracked undercarriage, ivory ceramic armor, brushed brass trim, crimson banners and military cloth accents, grimdark sci-fi crusader empire, massive battlefield war machine, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 30K + 삼각형 면`
- Remesh 올리는 기준: 차체 덩어리와 포탑 구조가 많이 깨지면 `고정형 + 100K + 삼각형 면`
- Texture 추천 문장:
```text
ivory ceramic armor, brushed brass trim, crimson banners, heavy industrial fortress finish, subtle weathering, grimdark sci-fi crusader empire
```
- Rigging: `사용 안 함`
- Animate: `사용 안 함`, 대신 Unity에서 `차체 이동`, `포탑 회전`, `발사 반동`, `발광 점멸` 구현
- Export: 기본 `FBX`
- Unity 메모: 너무 복잡하면 Blender에서 포탑만 분리해도 충분하다.

### 0-9. 플레이어 비행 거점 작업 카드
- 역할: 플레이어 진영 공중 요새형 대형 유닛
- 생성 설정: `Meshy 6`, `Standard`, `None`, `4 generations`, `Fixed Seed OFF`
- 선택 기준: `떠다니는 요새처럼 보이고`, `중앙 구조가 안정적이며`, `무장 배치가 읽히는 결과`
- 프롬프트:
```text
airborne citadel warship, giant hovering fortress silhouette, armored cathedral superstructure, anti-air batteries, heavy beam cannons, layered flying bastion hull, ivory ceramic plating, brushed brass trim, crimson lit windows and banners, grimdark sci-fi crusader empire, majestic but brutal military flagship, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```
- Remesh 첫 시도: `고정형 + 30K + 삼각형 면`
- Remesh 올리는 기준: 큰 형태와 무장 구조가 많이 깨지면 `고정형 + 100K + 삼각형 면`
- Texture 추천 문장:
```text
ivory ceramic plating, brushed brass trim, crimson lit windows, monumental fortress finish, subtle weathering, grimdark sci-fi crusader empire
```
- Rigging: `사용 안 함`
- Animate: `사용 안 함`, 대신 Unity에서 `부유`, `느린 기울기`, `무장 회전`, `발사 반동`, `엔진 발광 변화` 구현
- Export: 기본 `FBX`
- Unity 메모: 너무 큰 스케일로 들어오면 카메라를 가릴 수 있으니 크기를 먼저 본다.
## 가장 먼저 기억할 것
- 프롬프트는 `길수록 좋은 것`이 아니라 `헷갈리지 않게 명확한 것`이 좋다.
- 한 번에 `하나의 오브젝트`만 만든다.
- 캐릭터는 `T-Pose`, 병기/전투기/거점은 `None`이 기본이다.
- 첫 시도는 `4 generations`, 방향이 잡히면 `fixed seed ON`으로 다듬는다.
- 핵심 보병 4종이 먼저다: `창병`, `방패 보병`, `총병`, `포병`.

## 바로 복붙할 때의 사용 규칙
- 아래 `프롬프트:` 블록은 문장 일부만 떼지 말고 `통째로` 넣는다.
- 캐릭터는 `Pose: T-Pose`를 같이 맞춘다.
- 차량, 포병, 전투기, 거점은 `Pose: None`을 같이 맞춘다.
- 첫 시도는 `Fixed Seed OFF`로 4개를 보고, 가장 괜찮은 결과를 고른 다음에만 `Fixed Seed ON`으로 다시 돌린다.
- 결과가 이상하면 프롬프트를 더 길게 늘리지 말고, 맨 아래 `보정 문장` 중 하나만 끝에 추가한다.
- 그래도 흔들리면 `Text to 3D` 대신 기준 이미지를 만든 뒤 `Image to 3D`로 넘어간다.

## 권장 Export 포맷
- 이 프로젝트의 `기본 Export 포맷`은 `FBX`로 고정한다.
- 이유는 Unity 게임 작업에서 호환성이 좋고, Meshy 공식 가이드도 게임 개발 기본 선택으로 `FBX`를 우선 추천하기 때문이다.
- 특히 나중에 Blender 정리, 리깅, 애니메이션, Unity 재임포트를 생각하면 `FBX`가 가장 무난하다.
- `GLB`는 `빠른 미리보기용 보조 포맷`으로만 쓴다.
- Blender 정리 없이 Unity에 바로 테스트 반입할 때만 `GLB`를 예외적으로 우선 써도 된다.
- `OBJ`는 애니메이션이 없고 파이프라인 정보가 약해서 기본 포맷으로 잡지 않는다.
- `BLEND`는 Blender만 쓸 때는 편하지만, Meshy 다운로드 기본 포맷으로 고정하지 않는다.
- `STL`, `USDZ`는 이 프로젝트 기본 작업 흐름에서는 쓰지 않는다.

### 한 줄 결론
- Meshy에서 내려받을 때는 `FBX`가 기본이다.
- 텍스처 확인이나 빠른 테스트가 필요할 때만 `GLB`를 추가로 받는다.

### 추천 작업 흐름
1. Meshy 생성 완료
2. `FBX` 다운로드
3. Blender에서 정리하고 저장은 `.blend`
4. Unity에는 정리된 `FBX`를 가져간다
5. 텍스처 확인이 필요하면 같은 모델의 `GLB`도 같이 받아 비교한다

### 파일 관리 규칙
- `Meshy 원본 다운로드본`은 원본 보관용 폴더에 둔다.
- `Blender 작업본`은 `.blend`로 따로 저장한다.
- `Unity 최종 반입본`은 정리된 `FBX`를 사용한다.
- 즉 `Meshy 원본`, `Blender 작업본`, `Unity 반입본`을 섞지 않는다.

## Meshy 기본 설정 프리셋

### 인간형 유닛 기본 프리셋
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `T-Pose`
- Number of Generations: `4`
- Fixed Seed: `OFF` 첫 시도, `ON` 재시도부터

### 차량/거점 기본 프리셋
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `None`
- Number of Generations: `4`
- Fixed Seed: `OFF` 첫 시도, `ON` 재시도부터

### 전투기 기본 프리셋
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `None`
- Number of Generations: `4`
- Fixed Seed: `OFF` 첫 시도, `ON` 재시도부터

## 프롬프트 작성 공통 규칙
- `single object`
- `centered`
- `isolated`
- `symmetrical presentation`
- `no base`
- `no environment`
- `no background`
- `no text`
- `no logo`
- `no smoke`
- `no magic effects`
- `no floating debris`
- `no cropped body`
- `all major parts attached`
- `game-ready readable silhouette`

위 조건은 프롬프트 안에 포함해 두는 편이 좋다.

## 복붙용 공통 스타일 문장

### 플레이어 진영 스타일 잠금 문장
```text
grimdark sci-fi crusader empire, cathedral-inspired hard-surface armor, ivory ceramic plating, brushed brass trim, deep crimson cloth accents, heavy industrial military design, noble but brutal military silhouette, game-ready readable silhouette
```

### 적 진영 스타일 잠금 문장
```text
frontline firepower civilization, angular industrial sci-fi military design, dark gunmetal armor, burnt orange emissive accents, exposed weapon systems, aggressive battlefield silhouette, harsh militarized engineering, game-ready readable silhouette
```

## 사용 순서
1. 아래 프롬프트를 그대로 붙여넣는다.
2. 가장 마음에 드는 결과 1개를 고른다.
3. 그다음부터는 `fixed seed ON`으로 디테일만 수정한다.
4. 결과가 산만하면 프롬프트를 더 길게 쓰지 말고 장식 문장을 1~2개 줄인다.

## 1. 바로 복붙 프롬프트

### 1. 플레이어 창병
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `T-Pose`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
full body sci-fi crusader spearman, T-pose, symmetrical presentation, tall heavy infantry silhouette, long powered shock spear with blade tip held as part of the full model, broad plated shoulders, reinforced greaves, cathedral-inspired hard-surface armor, ivory ceramic plating, brushed brass trim, deep crimson cloth tabard, grimdark sci-fi crusader empire, noble but brutal military silhouette, clean hard-surface surfaces, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

### 2. 플레이어 방패 보병
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `T-Pose`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
full body sci-fi crusader shield infantry, T-pose, symmetrical presentation, broad defensive silhouette, massive rectangular energy shield mounted on one arm, compact sidearm holstered on the hip, layered chest armor, thick forearm guards, cathedral-inspired hard-surface armor, ivory ceramic armor, brushed brass trim, crimson cloth accents, grimdark sci-fi crusader empire, frontline defensive heavy infantry, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

### 3. 적 총병
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `T-Pose`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
full body sci-fi rifle infantry, T-pose, symmetrical presentation, lean aggressive firing-line silhouette, long industrial pulse rifle held as part of the full model, angular armored chest rig, compact backpack power unit, dark gunmetal armor, burnt orange emissive accents, exposed weapon systems, frontline firepower civilization, harsh militarized engineering, aggressive battlefield silhouette, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

### 4. 적 포병
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `None`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
sci-fi field artillery platform, low wide silhouette, oversized long-barrel siege cannon, reinforced recoil housing, heavy industrial armor plates, stabilizer legs, exposed loading mechanism, dark gunmetal body, burnt orange warning lights, frontline firepower civilization, harsh militarized engineering, brutal battlefield machine, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, no crew, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

### 5. 플레이어 특수전사
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `T-Pose`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
full body sci-fi elite shock warrior, T-pose, symmetrical presentation, compact athletic assault silhouette, twin energy blades attached to both hands, reinforced jump pack, segmented armored torso, black under-armor with ivory ceramic plating and brushed brass trim, crimson cloth accents, grimdark sci-fi crusader empire, elite assault infantry, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

### 6. 플레이어 친위대
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `T-Pose`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
full body royal guard sci-fi crusader, T-pose, symmetrical presentation, noble heavy elite silhouette, ceremonial power halberd held as part of the full model, integrated side shield, ornate cathedral armor, polished ivory armor, rich brushed brass trim, deep crimson royal cloth, imperial guardian presence, grimdark sci-fi crusader empire, elite palace battlefield guard, game-ready readable silhouette, single object, centered, isolated, full body visible, all major parts attached, no cropped body, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

### 7. 플레이어 전투기
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `None`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
sci-fi fighter aircraft, sharp forward-swept silhouette, twin engines, nose cannon, missile hardpoints, armored fuselage, cathedral-inspired military styling, ivory panels, brushed brass details, crimson markings, grimdark sci-fi crusader empire, fast readable combat aircraft, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, landing gear retracted, no base, no environment, no background, no text, no logo, no smoke trails, no magic effects, no floating debris
```

### 8. 플레이어 이동 거점
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `None`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
mobile fortress vehicle, colossal land battleship silhouette, layered armor citadel, multiple heavy turrets, cathedral-inspired command tower, massive tracked undercarriage, ivory ceramic armor, brushed brass trim, crimson banners and military cloth accents, grimdark sci-fi crusader empire, massive battlefield war machine, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

### 9. 플레이어 비행 거점
설정:
- AI Model: `Meshy 6`
- Model Type: `Standard`
- Pose: `None`
- Generations: `4`
- Fixed Seed: `OFF`

프롬프트:
```text
airborne citadel warship, giant hovering fortress silhouette, armored cathedral superstructure, anti-air batteries, heavy beam cannons, layered flying bastion hull, ivory ceramic plating, brushed brass trim, crimson lit windows and banners, grimdark sci-fi crusader empire, majestic but brutal military flagship, game-ready readable silhouette, single complete model, centered, isolated, all major parts attached, no base, no environment, no background, no text, no logo, no smoke, no magic effects, no floating debris
```

## 2. 짧은 작업 순서

### 1. 플레이어 창병
1. `Text to 3D`에서 창병 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. 결과 4개 중에서 `창이 분명하고`, `전신이 잘리고 있지 않고`, `실루엣이 가장 읽기 쉬운` 결과 1개를 고른다.
3. 마음에 드는 결과가 나오면 `텍스처`를 적용한다.
4. 인간형이므로 Meshy에서 `리깅`을 건다.
5. 애니메이션은 최소 `대기`, `걷기`, `달리기`, `공격`, `죽음`을 추가한다.
6. `FBX`로 내보낸다.
7. Unity에서 `Animator Controller`를 만들고 `Idle / Move / Attack / Death` 상태를 연결한다.

### 2. 플레이어 방패 보병
1. `Text to 3D`에서 방패 보병 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `방패가 크게 읽히는 결과`, `몸체가 너무 가늘지 않은 결과`, `무기와 방패가 빠지지 않은 결과`를 우선 선택한다.
3. 텍스처를 적용한다.
4. Meshy에서 `리깅`을 건다.
5. 애니메이션은 최소 `대기`, `걷기`, `방패 전진`, `공격`, `죽음`을 준비한다.
6. `FBX`로 내보낸다.
7. Unity에서는 방패 보병이므로 이동 속도를 낮게 잡고, 공격보다 `버티는 동작`이 잘 보이게 Animator를 연결한다.

### 3. 적 총병
1. `Text to 3D`에서 총병 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `총이 길고 분명한 결과`, `사격선 보병처럼 보이는 결과`, `허리가 너무 얇지 않은 결과`를 고른다.
3. 텍스처를 적용한다.
4. Meshy에서 `리깅`을 건다.
5. 애니메이션은 최소 `대기`, `걷기`, `달리기`, `사격 공격`, `죽음`을 추가한다.
6. `FBX`로 내보낸다.
7. Unity에서는 원거리 유닛이므로 이동 애니메이션과 사격 애니메이션 전환이 자연스럽게 이어지도록 Animator를 연결한다.

### 4. 적 포병
1. `Text to 3D`에서 포병 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `포신이 가장 잘 읽히는 결과`, `차체와 받침 구조가 안정적으로 보이는 결과`를 고른다.
3. 텍스처를 적용한다.
4. 포병은 인간형이 아니므로 Meshy 자동 리깅/애니메이션은 우선 생략한다.
5. `FBX`로 내보낸다.
6. Unity에서 `포신 반동`, `발사 시 흔들림`, `회전`은 코드나 간단한 Transform 애니메이션으로 구현한다.
7. 정말 필요할 때만 Blender에서 포신 반동 같은 짧은 보조 애니메이션을 만든다.

### 5. 플레이어 특수전사
1. `Text to 3D`에서 특수전사 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `쌍검이 분명한 결과`, `점프팩이 잘 붙어 있는 결과`, `몸이 지나치게 가늘지 않은 결과`를 고른다.
3. 텍스처를 적용한다.
4. Meshy에서 `리깅`을 건다.
5. 애니메이션은 최소 `대기`, `걷기`, `달리기`, `돌격 공격`, `죽음`을 추가한다.
6. 가능하면 `빠른 이동` 느낌이 있는 애니메이션을 고른다.
7. `FBX`로 내보내고 Unity에서 근접 돌격 유닛처럼 제어한다.

### 6. 플레이어 친위대
1. `Text to 3D`에서 친위대 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `할버드가 잘 보이는 결과`, `장중한 갑옷 실루엣이 있는 결과`, `왕실 근위대 느낌이 강한 결과`를 고른다.
3. 텍스처를 적용한다.
4. Meshy에서 `리깅`을 건다.
5. 애니메이션은 최소 `대기`, `걷기`, `공격`, `피격`, `죽음`을 추가한다.
6. 일반 보병보다 `무겁고 품위 있는 동작`이 보이는 쪽을 우선 선택한다.
7. `FBX`로 내보내고 Unity에서 엘리트 근접 유닛 Animator로 연결한다.

### 7. 플레이어 전투기
1. `Text to 3D`에서 전투기 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `기수와 날개 실루엣이 잘 읽히는 결과`, `좌우 균형이 좋은 결과`, `무장 포인트가 잘 드러나는 결과`를 고른다.
3. 텍스처를 적용한다.
4. 전투기는 인간형이 아니므로 Meshy 자동 리깅은 기본적으로 쓰지 않는다.
5. `FBX`로 내보낸다.
6. Unity에서 `전진`, `뱅킹`, `상승`, `하강`, `발사`를 코드로 제어한다.
7. 필요하면 Blender에서 `랜딩기어`, `캐노피`, `보조익` 정도만 보조 애니메이션으로 만든다.

### 8. 플레이어 이동 거점
1. `Text to 3D`에서 이동 거점 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `거대한 차체`, `포탑 구조`, `지상 이동 요새처럼 보이는 실루엣`이 가장 분명한 결과를 고른다.
3. 텍스처를 적용한다.
4. 이동 거점은 자동 리깅보다 `파츠 분리 후 코드 제어`가 더 현실적이다.
5. `FBX`로 내보낸다.
6. Unity에서 차체 이동, 포탑 회전, 발사 반동, 전조등이나 발광부 점멸을 코드로 제어한다.
7. 진짜 필요한 경우에만 Blender에서 포탑 반동이나 해치 개폐를 추가한다.

### 9. 플레이어 비행 거점
1. `Text to 3D`에서 비행 거점 프롬프트를 그대로 넣고 `4 generations`로 생성한다.
2. `공중 요새처럼 보이는 실루엣`, `중앙 구조가 안정적인 결과`, `무장 배치가 잘 읽히는 결과`를 고른다.
3. 텍스처를 적용한다.
4. 비행 거점도 인간형 애니메이션 대상이 아니므로 자동 리깅은 기본적으로 생략한다.
5. `FBX`로 내보낸다.
6. Unity에서 `부유`, `천천히 기울기`, `무장 회전`, `발사 반동`, `엔진 발광 변화`를 코드로 제어한다.
7. 필요할 때만 Blender에서 큰 구조물용 보조 애니메이션을 추가한다.

## 인간형과 비인간형의 작업 기준 요약
- `창병`, `방패 보병`, `총병`, `특수전사`, `친위대`는 `Meshy 생성 -> 텍스처 -> 리깅 -> 애니메이션 -> FBX -> Unity Animator` 흐름으로 간다.
- `포병`, `전투기`, `이동 거점`, `비행 거점`은 `Meshy 생성 -> 텍스처 -> FBX -> Unity 코드 제어`가 기본이다.
- 즉 인간형은 Meshy 애니메이션 활용 가치가 높고, 비인간형은 Unity 구현 비중이 더 크다.

## 최소 애니메이션 세트 추천
- 근접 인간형: `Idle`, `Walk`, `Run`, `Attack`, `Death`
- 원거리 인간형: `Idle`, `Walk`, `Run`, `Shoot`, `Death`
- 방패 계열: `Idle`, `Walk`, `Brace`, `Attack`, `Death`
- 비인간형 병기: Meshy 애니메이션보다 `Unity 코드 + 간단한 파츠 애니메이션` 우선
## 결과가 애매할 때 바로 쓰는 보정 문장

### 더 단단하고 묵직하게 만들고 싶을 때 프롬프트 끝에 추가
```text
very strong massing, heavier lower body, thicker armor plates, simplified large readable forms, less decoration, more military weight
```

### 너무 장난감처럼 보일 때 프롬프트 끝에 추가
```text
less toy-like, less cute, more grim militarized realism, harder edges, cleaner hard-surface armor logic
```

### 너무 복잡하고 지저분할 때 프롬프트 끝에 추가
```text
fewer small details, fewer layered ornaments, cleaner silhouette, simpler readable surface breakup, large primary forms first
```

### 진영 통일감을 더 강하게 잡고 싶을 때 프롬프트 끝에 추가
```text
same visual language as a unified faction asset set, consistent materials, consistent shape language, consistent military manufacturing style
```

## Text to 3D와 Image to 3D를 언제 쓰나

### Text to 3D
- 지금처럼 기준 유닛을 처음 만들 때
- 실루엣과 스타일 방향을 정할 때
- 여러 버전을 빠르게 비교할 때

### Image to 3D
- 같은 진영 유닛을 통일감 있게 맞추고 싶을 때
- 이미 만족스러운 기준 이미지가 있을 때
- Text to 3D 결과가 계속 흔들릴 때

## 이 프로젝트에서 특히 피할 것
- 전장 배경까지 같이 넣은 프롬프트
- 한 프롬프트에서 여러 유닛을 동시에 만들기
- 과한 에너지 이펙트와 연기 표현
- 너무 얇고 부서지기 쉬워 보이는 장식 위주 디자인
- 초반부터 모든 병종을 동시에 만들기

## 가장 추천하는 시작 순서
1. 플레이어 `창병`
2. 플레이어 `방패 보병`
3. 적 `총병`
4. 적 `포병`

이 4개가 먼저 나와야 전체 RTS 유닛 비주얼 언어가 잡힌다.

## 참고 링크
- Meshy Text to 3D 도움말
  - https://help.meshy.ai/en/articles/9996858-how-to-use-the-text-to-3d-feature
- Meshy Text to 3D 기능 페이지
  - https://www.meshy.ai/features/text-to-3d/
- Meshy Text to 3D API 문서
  - https://docs.meshy.ai/en/api/text-to-3d
- Meshy Image to 3D 도움말
  - https://help.meshy.ai/en/articles/9996860-how-to-use-the-image-to-3d-feature

## 참고 메모
- 위 설정과 팁은 공식 Meshy 도움말과 기능 페이지를 기준으로 정리했다.
- `유닛 세트 제작 순서`, `프롬프트 문장 구조`, `이 프로젝트용 스타일 문장`, `유닛별 최종 복붙 프롬프트`는 이 프로젝트 운영 기준에 맞춰 정리했다.



## 3. 상세 제작 매뉴얼

이 섹션은 위의 `요약 순서`를 실제 작업 체크리스트 수준으로 풀어쓴 것이다.
집에서 작업할 때는 아래 순서를 그대로 따라가면 된다.

### 1. 플레이어 창병 상세 매뉴얼

#### 목표
- 플레이어 진영의 가장 기본이 되는 `근접 보병 기준 모델`을 만든다.
- 이후 친위대, 특수전사, 방패 보병의 비주얼 기준점이 되므로 `너무 화려한 장식`보다 `실루엣과 무게감`을 우선한다.

#### Meshy 생성 전 설정
1. `AI Model`은 `Meshy 6`으로 둔다.
2. `Model Type`은 `Standard`를 선택한다.
3. `Pose`는 `T-Pose`를 선택한다.
4. `Generations`는 `4`로 둔다.
5. 첫 시도에서는 `Fixed Seed`를 끈다.

#### 프롬프트 입력 단계
1. 문서 상단의 `플레이어 창병` 최종 프롬프트를 처음부터 끝까지 통째로 복사한다.
2. 프롬프트 일부만 수정하지 말고 첫 생성은 그대로 돌린다.
3. 첫 결과가 나오기 전까지는 장식 문장 추가를 하지 않는다.

#### 생성 결과 선택 기준
1. 창이 짧거나 사라진 결과는 버린다.
2. 몸체보다 머리나 어깨가 지나치게 커 보이면 버린다.
3. 전신이 잘렸거나 망토가 몸에 과도하게 붙어 실루엣이 죽으면 버린다.
4. `창`, `양 어깨`, `다리 갑주`, `몸통 비율`이 가장 분명한 결과 1개를 고른다.
5. 갑옷 표면이 너무 울퉁불퉁하거나 천 재질이 과하게 많으면 고르지 않는다.

#### 생성 실패 시 수정 순서
1. 너무 마른 느낌이면 `very strong massing, heavier lower body` 보정 문장을 끝에 붙여 다시 돌린다.
2. 장식이 너무 많으면 `fewer small details, cleaner silhouette` 보정 문장을 붙인다.
3. 창이 여전히 약하면 프롬프트 앞쪽에 `very prominent long spear silhouette`를 추가한다.
4. 그래도 흔들리면 같은 프롬프트로 `Fixed Seed OFF` 상태에서 한 번 더 4세대를 본다.

#### 텍스처 단계
1. 형태가 가장 안정적인 결과를 고른 뒤 텍스처를 적용한다.
2. 아이보리와 황동 대비가 너무 약하면 다음 시도에서 색상 관련 문장을 조금 더 앞쪽으로 옮긴다.
3. 텍스처가 너무 더럽거나 낡아 보이면 첫 기준 유닛으로는 사용하지 않는다.
4. 이 모델은 진영 기준점이므로 `깨끗하지만 너무 새것 같지는 않은` 정도를 노린다.

#### 리깅 단계
1. 창병은 인간형이므로 Meshy의 `리깅`을 진행한다.
2. 리깅 후 팔, 손목, 무릎이 비정상적으로 꺾이는지 먼저 확인한다.
3. 창을 쥔 손이 무기에서 완전히 떨어져 보이면 해당 결과는 기준 모델로 쓰지 않는다.
4. 망토가 다리 사이를 심하게 관통하면 일단 보류하고 다른 생성 결과를 우선 검토한다.

#### 애니메이션 단계
1. 최소한 `Idle`, `Walk`, `Run`, `Attack`, `Death`를 넣는다.
2. 공격은 창 찌르기 계열이 가장 좋다.
3. 걷기와 달리기는 다리 간격이 과하게 넓지 않은 쪽을 고른다.
4. 죽음 애니메이션은 몸이 접히거나 무기가 몸을 지나치게 관통하지 않는 쪽을 고른다.
5. 같은 모델 안에서 동작 톤이 너무 다르면 그 모델은 기준 유닛으로 쓰기 어렵다.

#### Export 단계
1. 기본 다운로드는 `FBX`로 한다.
2. 빠른 확인용이 필요하면 `GLB`를 추가로 받는다.
3. 파일명은 `player_spearman_src_meshy`, `player_spearman_anim`, `player_spearman_export`처럼 구분해서 관리한다.

#### Unity 반입 단계
1. Unity에는 우선 `FBX`만 가져간다.
2. `Animator Controller`를 만들고 `Idle`, `Move`, `Attack`, `Death` 상태를 연결한다.
3. 이동과 공격 전환이 어색하면 애니메이션 자체보다 상태 전이 시간을 먼저 조절한다.
4. 창병은 기본 유닛이므로 속도, 공격 타이밍, 피격 반응 기준점으로 삼는다.

#### 최종 채택 기준
- 멀리서 봐도 창병인지 바로 읽혀야 한다.
- 플레이어 진영 기본 색과 재질이 잘 잡혀야 한다.
- 이후 다른 플레이어 유닛 프롬프트의 기준점으로 재사용 가능해야 한다.

### 2. 플레이어 방패 보병 상세 매뉴얼

#### 목표
- `버티는 전선 유닛` 역할이 눈에 들어오는 방패 보병을 만든다.
- 창병보다 더 넓고 무거운 실루엣이 필요하다.

#### Meshy 생성 전 설정
1. `Meshy 6`, `Standard`, `T-Pose`, `4 generations`를 유지한다.
2. 첫 시도는 `Fixed Seed OFF`로 한다.

#### 프롬프트 입력 단계
1. 문서의 `플레이어 방패 보병` 프롬프트를 그대로 넣는다.
2. 방패 크기와 실루엣이 중요하므로 임의로 문장을 줄이지 않는다.

#### 생성 결과 선택 기준
1. 방패가 몸보다 너무 작으면 버린다.
2. 방패만 지나치게 커서 몸 실루엣이 완전히 죽으면 버린다.
3. 측면에서 봤을 때도 두꺼운 전선 유닛처럼 느껴지는 결과를 고른다.
4. 한 손 무기나 권총이 너무 튀면 버린다. 주인공은 방패다.
5. 머리, 몸통, 방패, 다리 비율이 안정적인 1개를 고른다.

#### 생성 실패 시 수정 순서
1. 방패 존재감이 약하면 `very prominent shield silhouette`를 프롬프트 앞부분에 추가한다.
2. 너무 장난감 같으면 `less toy-like, harder edges` 보정 문장을 붙인다.
3. 장식이 많으면 `less decoration, simpler readable forms`를 붙인다.

#### 텍스처 단계
1. 방패 표면이 너무 복잡하면 기준 모델로 쓰지 않는다.
2. 몸체와 방패가 같은 진영 재질 언어를 공유하는지 본다.
3. 방패 가장자리 황동 장식은 있어도 되지만, 과하면 부담스럽다.

#### 리깅 단계
1. 리깅 후 방패를 든 팔이 비틀리지 않는지 본다.
2. 팔꿈치가 방패를 관통하면 다른 결과를 우선 검토한다.
3. 다리 길이가 너무 짧아 보이면 이동 애니메이션에서 어색해질 가능성이 높다.

#### 애니메이션 단계
1. 최소 `Idle`, `Walk`, `Brace`, `Attack`, `Death`를 준비한다.
2. `Brace`는 가능한 한 있으면 좋고, 없다면 `Idle`과 `Walk`만으로 임시 운용해도 된다.
3. 공격 동작은 방패 보병답게 짧고 묵직한 동작이 좋다.
4. 달리기는 없어도 되지만, 있다면 너무 가볍지 않아야 한다.

#### Export와 Unity 반입
1. `FBX`로 export 한다.
2. Unity에서는 이동 속도를 창병보다 낮게 잡는다.
3. Animator 전이도 빠른 돌격보다 `버티고 전진하는 느낌`에 맞춘다.
4. 방패 전면 방향이 유닛 전진 방향과 어긋나지 않는지 반드시 확인한다.

#### 최종 채택 기준
- 멀리서 봐도 `방패 보병`이라는 역할이 즉시 읽혀야 한다.
- 창병보다 넓고 무거워 보여야 한다.
- 플레이어 전선 유닛의 기준이 되어야 한다.

### 3. 적 총병 상세 매뉴얼

#### 목표
- 적 진영의 `기본 원거리 보병`을 만든다.
- 플레이어 보병과 구분되는 `각진 산업형 화력 실루엣`이 중요하다.

#### 생성 전 설정
1. `Meshy 6`, `Standard`, `T-Pose`, `4 generations`를 유지한다.
2. 첫 시도는 `Fixed Seed OFF`로 한다.

#### 프롬프트 입력 단계
1. 적 총병 최종 프롬프트를 그대로 넣는다.
2. 총기 실루엣과 백팩 전원부 문장이 빠지지 않게 한다.

#### 선택 기준
1. 총이 짧거나 몸에 묻혀 잘 안 보이면 버린다.
2. 플레이어 진영처럼 둥글고 성전 기사풍으로 나오면 버린다.
3. 가슴 장비와 총기, 어깨선이 각진 방향으로 읽히는 결과를 고른다.
4. 헬멧이나 고글이 있더라도 얼굴부가 너무 복잡하면 우선순위를 낮춘다.

#### 실패 시 수정 순서
1. 너무 플레이어 진영처럼 보이면 `angular`, `industrial`, `harsh militarized engineering` 문장을 앞쪽으로 옮긴다.
2. 총이 약하면 `very prominent rifle silhouette`를 앞부분에 넣는다.
3. 몸이 약하면 `more military weight`를 보정 문장으로 추가한다.

#### 텍스처 단계
1. 어두운 금속과 주황빛 포인트가 살아나는지 확인한다.
2. 발광 포인트가 너무 많아 장난감처럼 보이면 기준 모델로 쓰지 않는다.
3. 텍스처가 너무 화려하면 적군 통일감이 깨질 수 있다.

#### 리깅과 애니메이션
1. 총병은 Meshy에서 리깅을 진행한다.
2. 최소 `Idle`, `Walk`, `Run`, `Shoot`, `Death`를 준비한다.
3. 사격 애니메이션은 총구 방향이 이상하게 꺾이지 않는지 본다.
4. 걷기에서 어깨와 총이 지나치게 흔들리면 전열 보병 느낌이 약해진다.

#### Export와 Unity 반입
1. `FBX`로 export 한다.
2. Unity에서는 `Move`와 `Shoot` 사이 전환이 중요하다.
3. 총구 플래시, 탄환, 사거리 판정은 애니메이션보다 게임 로직이 우선이다.
4. 즉 사격 동작은 보기 좋게, 판정은 코드로 정확하게 처리한다.

#### 최종 채택 기준
- 플레이어 보병과 한눈에 적대 진영으로 구분돼야 한다.
- 총이 확실히 읽혀야 한다.
- 전열 사격 유닛처럼 보여야 한다.

### 4. 적 포병 상세 매뉴얼

#### 목표
- 적 진영의 `중화기 실루엣`을 담당하는 포병을 만든다.
- 사람형보다 병기 역할이 먼저 읽혀야 한다.

#### 생성 전 설정
1. `Meshy 6`, `Standard`, `Pose None`, `4 generations`를 사용한다.
2. 포병은 인간형이 아니므로 처음부터 T-pose를 쓰지 않는다.

#### 프롬프트 입력 단계
1. 적 포병 프롬프트를 그대로 넣는다.
2. 차체, 포신, 반동 구조가 빠지지 않았는지 확인한다.

#### 선택 기준
1. 포신이 짧거나 두께가 약하면 버린다.
2. 받침 구조가 너무 가늘어 장난감 같으면 버린다.
3. 차체와 포신이 다른 물체처럼 따로 노는 느낌이면 버린다.
4. 가장 안정적으로 `포격 병기`처럼 보이는 결과를 고른다.

#### 실패 시 수정 순서
1. 너무 빈약하면 `heavier armor plates, stronger massing`을 추가한다.
2. 장식이 많으면 `simplified large readable forms`를 붙인다.
3. 포신이 약하면 `very large siege cannon barrel`을 앞부분에 넣는다.

#### 텍스처 단계
1. 검은 금속 질감과 경고등 포인트 정도만 살아 있어도 충분하다.
2. 너무 많은 발광부는 피한다.
3. 병기답게 기능이 먼저 읽히는 텍스처가 좋다.

#### 애니메이션 처리 원칙
1. 포병은 Meshy 자동 리깅보다 Unity 코드 제어를 기본으로 한다.
2. 필요한 움직임은 `몸체 회전`, `포신 상하 각도`, `발사 반동`, `재장전 느낌` 정도다.
3. 이 중 대부분은 Unity Transform 제어로 충분하다.
4. Blender는 정말 필요할 때만 반동 보조 애니메이션을 만들 때 쓴다.

#### Export와 Unity 반입
1. `FBX`로 export 한다.
2. Unity에서 차체 기준축과 포신 기준축을 먼저 확인한다.
3. 포신 방향이 로컬 축 기준으로 예측 가능해야 코드 제어가 편하다.
4. 발사 반동은 애니메이션보다 `짧은 위치 이동 + 복귀`만으로도 충분하다.

#### 최종 채택 기준
- 멀리서 봐도 포대인지 읽혀야 한다.
- 적 진영 금속/산업 디자인 언어가 유지돼야 한다.
- 코드로 조작하기 쉬운 단순한 구조가 좋다.

### 5. 플레이어 특수전사 상세 매뉴얼

#### 목표
- 플레이어 진영의 `빠른 돌격 엘리트`를 만든다.
- 창병보다 민첩해 보이지만 같은 진영임이 분명해야 한다.

#### 생성 전 설정
1. `Meshy 6`, `Standard`, `T-Pose`, `4 generations`를 사용한다.
2. 첫 시도는 `Fixed Seed OFF`다.

#### 선택 기준
1. 쌍검이 빠지면 버린다.
2. 점프팩이 과하게 커서 실루엣이 산만하면 버린다.
3. 너무 마른 암살자처럼 보이면 버린다. 이 진영은 여전히 중량감이 필요하다.
4. 갑옷이 플레이어 진영 언어를 유지하면서도 더 기동적으로 보이는 결과를 고른다.

#### 텍스처와 리깅
1. 검은 언더슈트와 아이보리 판금 대비가 살아나는지 본다.
2. 리깅 후 어깨, 손목, 무릎이 공격 포즈에서 부자연스럽게 꺾이지 않는지 본다.
3. 쌍검이 손에 자연스럽게 붙어 있는지 확인한다.

#### 애니메이션
1. 최소 `Idle`, `Walk`, `Run`, `Attack`, `Death`를 넣는다.
2. 가능하면 `대시형`, `빠른 근접 공격형` 모션을 고른다.
3. 너무 곡예적인 동작은 RTS에서 과해 보일 수 있어 피한다.

#### Unity 반입
1. 기본 구조는 창병과 같지만 속도와 공격 전이 시간이 더 짧아야 한다.
2. 실루엣이 빠르게 움직여도 무기가 읽혀야 한다.
3. 너무 작은 모델이면 전장에서 존재감이 사라질 수 있으니 스케일도 같이 점검한다.

#### 최종 채택 기준
- 플레이어 진영 엘리트로 읽혀야 한다.
- 빠르지만 가볍지는 않아야 한다.
- 쌍검과 점프팩이 핵심 정체성으로 남아야 한다.

### 6. 플레이어 친위대 상세 매뉴얼

#### 목표
- 플레이어 진영의 `최상위 근접 엘리트` 느낌을 만든다.
- 창병보다 고급스럽고, 특수전사보다 더 장중해야 한다.

#### 선택 기준
1. 할버드가 선명하게 보여야 한다.
2. 어깨와 흉갑이 장중하게 읽혀야 한다.
3. 너무 화려해서 실루엣이 깨지면 버린다.
4. 왕실 근위대 느낌이 없고 그냥 일반 창병 업그레이드처럼 보이면 우선순위를 낮춘다.

#### 텍스처 단계
1. 황동 장식과 붉은 천은 창병보다 조금 더 많아도 된다.
2. 하지만 장식이 지나쳐 본체 실루엣이 죽으면 안 된다.
3. 광택이 너무 강하면 장난감처럼 보일 수 있다.

#### 리깅과 애니메이션
1. Meshy 리깅을 적용한다.
2. 최소 `Idle`, `Walk`, `Attack`, `Hit`, `Death`를 준비한다.
3. 일반 보병보다 한 템포 느리고 무거운 동작이 좋다.
4. 무기가 길기 때문에 공격 시 손과 무기의 결합이 자연스러운지 특히 본다.

#### Unity 반입
1. Animator 상태 수는 많지 않아도 된다.
2. 대신 `공격 모션의 무게감`과 `대기 상태의 존재감`이 중요하다.
3. 친위대는 화면에서 고급 유닛처럼 보여야 하므로 실루엣 유지가 최우선이다.

#### 최종 채택 기준
- 일반 보병과 차별화된 위엄이 있어야 한다.
- 너무 복잡하지 않으면서도 정예 느낌이 살아 있어야 한다.
- 플레이어 진영의 상징 유닛으로 재사용 가능해야 한다.

### 7. 플레이어 전투기 상세 매뉴얼

#### 목표
- 플레이어 진영 공중 전력의 기준 비주얼을 만든다.
- 캐릭터형 애니메이션보다 `기체 실루엣`과 `코드 제어 용이성`이 중요하다.

#### 생성 전 설정
1. `Meshy 6`, `Standard`, `Pose None`, `4 generations`를 쓴다.
2. 전투기는 인간형이 아니므로 리깅을 기본 전제로 두지 않는다.

#### 선택 기준
1. 좌우 대칭이 무너지면 버린다.
2. 날개와 동체 연결이 불안정하면 버린다.
3. 기수 방향이 분명하지 않으면 버린다.
4. 무장 하드포인트가 읽히는 결과를 고른다.
5. 너무 장식적인 성당 요소가 많아 비행체 기능이 묻히면 버린다.

#### 텍스처 단계
1. 아이보리 주색, 황동 보조, 붉은 표식 정도면 충분하다.
2. 발광은 많지 않아야 한다.
3. 빠르게 지나갈 때 색면이 단순하게 읽히는 편이 좋다.

#### 애니메이션 처리 원칙
1. 전투기는 Meshy 자동 애니메이션보다 Unity 코드 제어가 기본이다.
2. 필요한 것은 `전진`, `회전`, `뱅킹`, `상승/하강`, `발사`다.
3. 랜딩기어가 꼭 필요하면 Blender에서 따로 만든다.
4. 하지만 MVP 단계에서는 랜딩기어 없이도 충분하다.

#### Export와 Unity 반입
1. `FBX`로 export 한다.
2. Unity에서 기수 방향이 전방 축과 맞는지 먼저 확인한다.
3. 비행 제어는 Rigidbody보다 단순 이동 보간부터 시작해도 된다.
4. 발사 시 약한 기체 흔들림 정도만 넣어도 충분히 비행체처럼 느껴진다.

#### 최종 채택 기준
- 멀리서 봐도 전투기처럼 읽혀야 한다.
- 플레이어 진영 언어는 유지하되 과장된 장식은 적어야 한다.
- 코드로 회전과 이동을 제어하기 쉬워야 한다.

### 8. 플레이어 이동 거점 상세 매뉴얼

#### 목표
- 지상 이동 요새 같은 `거대 유닛`의 기준 모델을 만든다.
- 세부보다 `거대한 차체 + 포탑 + 전진하는 성채` 이미지가 중요하다.

#### 생성 전 설정
1. `Meshy 6`, `Standard`, `Pose None`, `4 generations`를 사용한다.
2. 사람형처럼 리깅하는 대상이 아니라는 점을 전제로 한다.

#### 선택 기준
1. 차체가 너무 짧거나 낮으면 버린다.
2. 포탑이 너무 많아 복잡하면 버린다.
3. 하부 구조가 지나치게 가늘면 버린다.
4. 실루엣만 봐도 `이동하는 요새` 느낌이 나는 결과를 고른다.

#### 텍스처 단계
1. 지나친 표면 정보보다 큰 색면 구분이 중요하다.
2. 차체와 포탑 구분이 읽혀야 한다.
3. 현수막이나 붉은 포인트는 소량이면 충분하다.

#### 애니메이션 처리 원칙
1. 이동 거점은 `차체 이동`, `포탑 회전`, `발사 반동`, `발광 점멸` 정도만 있어도 충분하다.
2. 대부분 Unity 코드로 처리한다.
3. 포탑이 여러 개라면 처음부터 모든 포탑을 다 움직이려 하지 않는다.
4. MVP 단계에서는 대표 포탑 1개만 살아 있어도 된다.

#### Export와 Unity 반입
1. `FBX`로 export 한다.
2. Unity 반입 후 포탑 기준축과 차체 기준축을 확인한다.
3. 너무 복잡한 메시라면 Blender에서 포탑 분리만 먼저 해도 좋다.
4. 화면에서 너무 커져 조작을 가리면 스케일을 줄인다.

#### 최종 채택 기준
- 거대한 이동 요새라는 정체성이 분명해야 한다.
- 세부보다 형태가 먼저 읽혀야 한다.
- Unity에서 최소 제어가 가능한 구조여야 한다.

### 9. 플레이어 비행 거점 상세 매뉴얼

#### 목표
- 공중에 떠 있는 대형 성채형 함선을 만든다.
- 이동 거점보다 더 거대하지만, 실루엣은 더 단순해야 한다.

#### 생성 전 설정
1. `Meshy 6`, `Standard`, `Pose None`, `4 generations`를 사용한다.
2. 자동 리깅이 아니라 `구조물형 대형 유닛`으로 접근한다.

#### 선택 기준
1. 중앙 몸체가 불안정해 보이면 버린다.
2. 무장이 너무 많아 전체 형태가 흐려지면 버린다.
3. 하부 실루엣이 약해 부유체처럼 안 보이면 버린다.
4. 멀리서도 `떠다니는 요새`처럼 읽히는 결과를 고른다.

#### 텍스처 단계
1. 창문 발광, 황동 장식, 붉은 포인트는 소량만 둔다.
2. 너무 화려하면 작은 전투기와 시각적으로 충돌한다.
3. 전체 톤은 `위엄`, `거대함`, `단순한 색면` 쪽이 좋다.

#### 애니메이션 처리 원칙
1. Unity 코드로 `천천히 부유`, `약한 롤`, `무장 회전`, `발사 반동`, `엔진 발광 변화`를 준다.
2. 구조물이 큰 만큼 작은 흔들림만 있어도 충분하다.
3. 큰 움직임을 주면 오히려 장난감처럼 보일 수 있다.
4. Blender 보조 애니메이션은 진짜 필요할 때만 쓴다.

#### Export와 Unity 반입
1. `FBX`로 export 한다.
2. Unity에서 피벗이 중앙에 잡혀 있는지 확인한다.
3. 무장 회전 포인트가 필요하면 Blender에서 부분 분리를 고려한다.
4. 카메라를 너무 가리지 않도록 실제 전장에서 화면 점유율도 테스트한다.

#### 최종 채택 기준
- 거대 공중 요새로 읽혀야 한다.
- 장식보다 위압적인 실루엣이 우선이다.
- Unity에서 부유 연출과 발사 연출을 붙이기 쉬워야 한다.

## 작업할 때 항상 지킬 공통 체크리스트
1. 첫 생성은 프롬프트를 줄이거나 늘리지 말고 그대로 쓴다.
2. 첫 시도는 `4 generations`, `Fixed Seed OFF`로 한다.
3. 모델을 고를 때는 `디테일`보다 `실루엣`, `비율`, `역할 인식`을 우선한다.
4. 인간형은 `리깅과 기본 애니메이션`까지 보고 채택한다.
5. 비인간형은 `코드 제어가 쉬운 구조`인지 먼저 본다.
6. 기본 Export는 `FBX`다.
7. Unity에 넣었을 때 멀리서도 역할이 읽히는지 꼭 확인한다.
8. 하나가 확정되면 그다음 유닛은 그 모델을 기준점으로 스타일을 맞춘다.

## 4. Meshy 기능 설명

이 섹션은 Meshy 안에서 자주 보게 되는 `Remesh`, `Texture`, `Texture Edit`, `Animate`, `Download` 기능을 이 프로젝트 기준으로 언제 쓰는지 정리한 것이다.
중요한 핵심 순서는 아래 한 줄로 먼저 기억하면 된다.

`생성 -> 결과 선택 -> Remesh -> Texture -> Texture Edit -> Animate -> Export -> Blender 정리 -> Unity 반입`

이 순서에서 특히 중요한 점은 `Remesh를 Texture보다 먼저` 검토하는 것이다.
Meshy 공식 도움말도 `더 좋은 UV 정렬 결과를 위해 Remesh를 텍스처 전에 하는 것을 보통 권장`하고 있다.

### 1. Remesh를 언제 쓰는가
- 생성된 메시가 너무 무겁거나 지나치게 지저분할 때 쓴다.
- 게임용으로 폴리 수를 줄이고 싶을 때 쓴다.
- 더 예측 가능한 메시 구조가 필요할 때 쓴다.
- `Quad` 위주의 결과가 필요할 때 쓴다.
- 특정 포맷으로 다시 다운로드하고 싶을 때도 Remesh를 거칠 수 있다.

### 2. Remesh를 언제 안 써도 되는가
- 첫 생성 결과가 이미 충분히 단순하고 깔끔할 때
- 아주 잠깐 테스트만 할 모델일 때
- 곧 Blender에서 직접 손볼 예정이라 Meshy 안에서 추가 비용을 쓰고 싶지 않을 때

### 3. Remesh 실제 작업 순서
1. 모델 생성 결과 중 가장 괜찮은 1개를 먼저 고른다.
2. Meshy 작업 화면에서 해당 모델을 선택한다.
3. `Remesh`를 연다.
4. `Topology`를 고른다.
5. `Target Polycount`를 정한다.
6. 필요한 `target format`이 있으면 같이 확인한다.
7. Remesh 결과를 보고 메시 밀도와 실루엣이 망가지지 않았는지 확인한다.
8. 괜찮으면 그 리메시 결과를 기준으로 다음 단계인 `Texture`로 간다.

### 4. Remesh의 Topology를 어떻게 고를까
- `Triangle`
  - Unity에 바로 넣을 게임용 테스트에서는 가장 무난하다.
  - 속도와 단순 반입 기준으로는 이쪽이 편하다.
- `Quad`
  - Blender에서 후속 수정, 리토폴로지, 표면 정리가 더 필요할 때 유리하다.
  - 인간형 캐릭터를 손으로 더 다듬고 싶을 때 고려한다.

이 프로젝트 기준 추천은 아래와 같다.
- 인간형 기준 모델: `Quad` 또는 깔끔한 `Triangle`
- 비인간형 병기: `Triangle`
- Blender 추가 수정이 거의 확실한 모델: `Quad`
- 빠른 Unity 테스트용 모델: `Triangle`

### 5. 이 프로젝트용 Remesh 권장값
이 숫자는 Meshy 공식 필수값이 아니라 `이 프로젝트 운영 기준 추천값`이다.

- 인간형 기본 보병: `10k~25k` 폴리 정도부터 시작
- 엘리트 인간형: `15k~35k` 폴리 정도부터 시작
- 포병/소형 병기: `8k~20k` 폴리 정도부터 시작
- 전투기: `12k~30k` 폴리 정도부터 시작
- 이동 거점/비행 거점: `25k~80k` 폴리 정도부터 시작

처음부터 숫자를 너무 높게 잡지 않는 편이 좋다.
우선 `실루엣이 무너지지 않는 최소 수준`을 찾고, 부족하면 올리는 방식이 안전하다.

### 5-1. 지금 보이는 Remesh 창 기준으로 고르는 법
현재 스크린샷 기준으로 Remesh 창에는 아래 선택지가 보인다.
- `폴리곤 수`: `고정형` / `적응형`
- `맞춤형`: `3K`, `10K`, `30K`, `100K`
- `위상`: `사각형 면` / `삼각형 면`

이 문단은 `지금 보이는 UI 기준 실전 선택 가이드`다.

### 5-2. 폴리곤 수의 `고정형`과 `적응형`은 어떻게 고르나
- `고정형`
  - 목표 폴리 수를 대략 명확하게 맞추고 싶을 때 쓴다.
  - Unity 테스트용으로 자산 크기를 일정하게 관리하기 쉽다.
  - 지금 프로젝트처럼 RTS 유닛 수가 많아질 수 있는 경우 기본 선택으로 가장 무난하다.
- `적응형`
  - 디테일이 많은 부분은 더 살리고 단순한 부분은 더 줄이려는 쪽에 가깝다.
  - 실루엣 보존이 더 중요할 때 고려할 만하다.
  - 다만 결과를 예측하기가 `고정형`보다 조금 덜 직관적일 수 있다.

이 프로젝트 기본 추천은 아래와 같다.
- 첫 리메시는 `고정형` 우선
- 실루엣이 자꾸 무너지면 그때 `적응형` 비교

### 5-3. 인간형 리메시 추천값
#### 창병, 방패 보병, 총병
- `폴리곤 수`: `고정형`
- 첫 시도 값: `10K`
- 결과가 너무 뭉개지면: `30K`
- `위상`: Blender 후속 수정이 있으면 `사각형 면`, Unity 빠른 테스트면 `삼각형 면`

이유:
- 기본 보병은 전장에 많이 나올 수 있어서 너무 무거우면 안 된다.
- 하지만 팔, 다리, 어깨, 무기 실루엣은 살아야 하므로 `3K`는 너무 공격적으로 줄어들 가능성이 높다.
- 그래서 보통 시작점은 `10K`가 무난하다.

#### 특수전사, 친위대 같은 엘리트 인간형
- `폴리곤 수`: `고정형`
- 첫 시도 값: `30K`
- 더 가볍게 돌릴 테스트용: `10K`
- `위상`: 가능하면 `사각형 면`

이유:
- 엘리트 유닛은 장식과 갑옷 실루엣이 조금 더 중요하다.
- 쌍검, 할버드, 추가 장갑 같은 요소가 `10K`에서 과하게 무너질 수 있다.
- 그래서 기준 모델 확보 목적이라면 `30K`부터 보는 편이 안전하다.

#### 인간형에서 `3K`와 `100K`는 언제 쓰나
- `3K`
  - 아주 멀리서만 볼 임시 테스트용
  - 대량 배치 실험용
  - 최종 기준 모델용으로는 보통 권장하지 않음
- `100K`
  - Meshy 안에서 형태 확인용 고디테일 실험
  - Blender에서 아주 많이 손볼 전제일 때
  - Unity RTS 프로토타입 기준으로는 대체로 무거운 편

### 5-4. 비인간형 리메시 추천값
#### 포병, 소형 병기
- `폴리곤 수`: `고정형`
- 첫 시도 값: `10K`
- 포신/받침 구조가 많이 무너지면: `30K`
- `위상`: 기본은 `삼각형 면`

이유:
- 병기형은 관절 리깅보다 차체와 포신 실루엣이 더 중요하다.
- Unity 코드 제어가 중심이므로 `삼각형 면`이 보통 더 편하다.

#### 전투기
- `폴리곤 수`: `고정형`
- 첫 시도 값: `10K` 또는 `30K`
- `위상`: 기본은 `삼각형 면`

이유:
- 전투기는 빠르게 움직이기 때문에 표면 미세 디테일보다는 전체 실루엣이 중요하다.
- 날개 끝, 동체, 엔진부가 무너지지 않으면 `10K`도 충분할 수 있다.
- 기체 장식이 많거나 동체가 복잡하면 `30K`로 올린다.

#### 이동 거점, 비행 거점
- `폴리곤 수`: `고정형`
- 첫 시도 값: `30K`
- 구조가 너무 크고 복잡하면: `100K`로 한 번 비교
- `위상`: 기본은 `삼각형 면`

이유:
- 대형 유닛은 파츠가 많고 큰 형상이 겹쳐 있어서 너무 낮은 폴리에서는 형태가 쉽게 죽는다.
- 다만 이 프로젝트는 프로토타입이므로 `100K`를 기본값으로 잡지는 않는 편이 좋다.
- 먼저 `30K`를 보고, 정말 필요할 때만 `100K`를 비교한다.

### 5-5. `사각형 면`과 `삼각형 면`을 지금 프로젝트에서 어떻게 고르나
- `사각형 면`
  - 인간형 캐릭터
  - Blender에서 나중에 더 손볼 모델
  - 리깅과 표면 정리를 같이 생각하는 모델
- `삼각형 면`
  - 포병, 전투기, 거점 같은 병기형
  - Unity에 빠르게 넣어볼 테스트 자산
  - 코드 제어가 핵심인 모델

한 줄 기준으로 말하면:
- `인간형 = 사각형 면 우선`
- `병기형 = 삼각형 면 우선`

### 5-6. 스크린샷 같은 인간형 창병에는 무엇을 고르면 되나
현재 보이는 창병 같은 인간형 기준 추천은 아래다.
1. 첫 시도: `고정형 + 10K + 사각형 면`
2. 창, 손, 망토, 어깨가 너무 무너지면: `고정형 + 30K + 사각형 면`
3. Blender 수정 없이 Unity에 빠르게 넣어볼 때만: `고정형 + 10K + 삼각형 면`

### 5-7. 병기형에는 무엇을 고르면 되나
#### 포병
1. 첫 시도: `고정형 + 10K + 삼각형 면`
2. 포신이나 받침 구조가 무너지면: `고정형 + 30K + 삼각형 면`

#### 전투기
1. 첫 시도: `고정형 + 10K + 삼각형 면`
2. 날개 끝이나 엔진부가 뭉개지면: `고정형 + 30K + 삼각형 면`

#### 이동 거점 / 비행 거점
1. 첫 시도: `고정형 + 30K + 삼각형 면`
2. 큰 형태가 너무 깨지면: `고정형 + 100K + 삼각형 면`
3. Blender에서 구조 정리를 오래 할 생각이면: `고정형 + 30K 또는 100K + 사각형 면`

### 5-8. 적응형은 언제 비교해볼 만한가
- 인간형에서 얼굴이나 손은 살고 큰 갑옷 면은 정리되길 원할 때
- 전투기에서 엔진 주변은 살리고 동체 평면은 줄이고 싶을 때
- 대형 거점에서 핵심 포탑과 실루엣만 살리고 싶을 때

다만 첫 시도부터 `적응형`으로 시작하기보다는, 같은 모델을 `고정형`으로 먼저 본 다음 비교하는 편이 더 판단하기 쉽다.
### 6. Remesh 결과를 볼 때 체크할 것
1. 머리, 어깨, 무기, 포신 같은 큰 형태가 무너지지 않았는가
2. 갑옷 모서리나 기체 모서리가 지나치게 둥글어지지 않았는가
3. 얇은 파츠가 끊기거나 찌그러지지 않았는가
4. 처음 모델보다 실루엣이 읽기 쉬워졌는가
5. Unity에서 다루기 쉬울 만큼 단순해졌는가

### 7. Texture를 언제 쓰는가
- 형태가 확정된 뒤 색과 재질을 입히고 싶을 때 쓴다.
- 무텍스처 모델을 게임 비주얼 기준에 맞추고 싶을 때 쓴다.
- 이미 텍스처가 있는 모델도 다른 방향으로 다시 재질화하고 싶을 때 쓴다.

### 8. Texture를 언제 미루는가
- 아직 메시 선택이 끝나지 않았을 때
- 곧 Remesh를 할 예정일 때
- 인간형이라도 형태가 흔들리는 상태일 때

즉, 이 프로젝트에서는 `형태 확정 -> Remesh 검토 -> Texture` 순서가 기본이다.

### 9. Texture 실제 작업 순서
1. Remesh까지 끝난 모델을 선택한다.
2. `Texture` 탭으로 간다.
3. 텍스처 프롬프트를 넣는다.
4. 가능하면 첫 시도는 기본 프롬프트를 크게 바꾸지 않는다.
5. 결과가 나오면 색, 재질, 명암, 표면 분리감, 진영 통일감을 본다.
6. 괜찮으면 그 버전을 기준으로 저장한다.
7. 일부만 이상하면 전체를 다시 돌리지 말고 `Texture Edit`로 간다.

### 10. Texture 프롬프트를 어떻게 써야 하나
- 모델 프롬프트와 똑같이 길게 쓸 필요는 없다.
- 텍스처 단계에서는 `색`, `재질`, `표면 느낌`, `오염 정도`, `진영 스타일`이 핵심이다.
- 형태 설명보다 재질 설명이 더 중요하다.

예시:
- 플레이어 진영 텍스처 문장
```text
ivory ceramic armor, brushed brass trim, deep crimson cloth accents, clean but battle-worn military finish, subtle edge wear, grimdark sci-fi crusader empire
```

- 적 진영 텍스처 문장
```text
dark gunmetal armor, burnt orange emissive accents, industrial military metal panels, heat-stained weapon surfaces, harsh utilitarian battlefield finish
```

### 11. Texture 결과를 볼 때 체크할 것
1. 색이 진영과 맞는가
2. 재질이 `금속`, `천`, `세라믹`처럼 구분되는가
3. 발광부가 너무 많아 장난감처럼 보이지 않는가
4. 오염 표현이 너무 심해 실루엣을 죽이지 않는가
5. 가까이서만 예쁘고 멀리서는 뭉개지지 않는가

### 12. Texture Edit를 언제 쓰는가
- 전체 모델은 괜찮은데 특정 부위 색이나 재질만 이상할 때
- 방패만 다시 칠하고 싶을 때
- 검만 금속 질감으로 바꾸고 싶을 때
- 발광부만 줄이고 싶을 때
- 망토 색만 조절하고 싶을 때

### 13. Texture Edit 실제 작업 순서
1. 텍스처가 적용된 모델을 선택한다.
2. `Texture Edit`로 들어간다.
3. `Brush`나 `Lasso`로 수정할 부위를 선택한다.
4. 프롬프트를 짧고 명확하게 쓴다.
5. 필요하면 `Prompt Influence Strength`를 조절한다.
6. 결과로 나오는 여러 버전 중 가장 맞는 것을 미리 본다.
7. 마음에 들면 `Apply`한다.
8. 최종적으로 괜찮으면 `Save to Model`로 새 모델 버전을 만든다.

중요한 점은 `Texture Edit는 원본을 덮어쓰지 않고 새 모델 버전으로 저장`한다는 것이다.
그래서 무리하게 한 번에 끝내려 하지 말고, 작은 부위 단위로 수정하는 편이 안전하다.

### 14. Texture Edit에서 잘 먹히는 수정 예시
- 방패만 더 밝은 상아색으로 바꾸기
- 검날만 더 차가운 금속으로 바꾸기
- 망토만 더 진한 붉은색으로 바꾸기
- 적 총병의 발광부만 줄이기
- 포신의 열변색을 약하게 넣기

### 15. Texture Edit에서 피할 것
- 한 번에 너무 넓은 면적을 선택하기
- 프롬프트를 장문 소설처럼 쓰기
- 작은 부위 수정인데 전체 리텍스처처럼 강한 문장을 넣기
- 이미 괜찮은 모델을 과하게 계속 덧수정하기

### 16. Texturing 관련 공식 팁을 이 프로젝트에 적용하면
- 텍스처 전에 `Remesh`를 먼저 검토한다.
- 텍스처 이상은 전체 재생성보다 `Texture Edit`로 먼저 고친다.
- Meshy 5 이상에서는 `de-lit textures`가 기본이므로, 불필요한 조명 제거 작업을 외부에서 따로 할 필요가 줄어든다.

### 17. Animate를 언제 쓰는가
- 인간형 유닛이 형태와 텍스처까지 어느 정도 확정됐을 때
- T-Pose 또는 A-Pose로 생성된 캐릭터일 때
- Unity Animator로 연결할 기본 동작이 필요할 때

### 18. Animate를 바로 쓰지 말아야 하는 경우
- 아직 모델 형태가 불안정할 때
- 텍스처나 실루엣이 마음에 들지 않을 때
- 포병, 전투기, 이동 거점, 비행 거점처럼 비인간형 병기일 때

### 19. Animate 실제 작업 순서
1. 인간형 모델을 선택한다.
2. `Rigging`을 실행한다.
3. 관절이 이상하게 꺾이는지 먼저 확인한다.
4. 괜찮으면 라이브러리에서 필요한 애니메이션을 추가한다.
5. 최소 세트는 `Idle`, `Walk`, `Run`, `Attack or Shoot`, `Death`다.
6. 여러 애니메이션이 쌓였으면 필요한 방식으로 다운로드한다.
7. Unity에서는 Animator 상태 이름을 단순하게 유지한다.

### 20. 기능 순서를 이 프로젝트 기준으로 다시 정리하면
- 인간형 기준 유닛
  - `Text to 3D -> 결과 선택 -> Remesh -> Texture -> Texture Edit -> Rig -> Animate -> FBX Export -> Unity`
- 비인간형 기준 유닛
  - `Text to 3D -> 결과 선택 -> Remesh -> Texture -> Texture Edit -> FBX Export -> Unity 코드 제어`

### 21. 모델별 기능 사용 우선순위
- 창병
  - `Remesh 중요`, `Texture 중요`, `Animate 중요`
- 방패 보병
  - `Remesh 중요`, `Texture 중요`, `Animate 중요`
- 적 총병
  - `Remesh 중요`, `Texture 중요`, `Animate 중요`
- 적 포병
  - `Remesh 중요`, `Texture 중요`, `Animate 낮음`, `Unity 코드 제어 높음`
- 특수전사
  - `Remesh 중요`, `Texture 중요`, `Animate 중요`
- 친위대
  - `Remesh 중요`, `Texture 중요`, `Animate 중요`
- 전투기
  - `Remesh 중요`, `Texture 중요`, `Animate 낮음`, `Unity 코드 제어 높음`
- 이동 거점
  - `Remesh 중요`, `Texture 중요`, `Animate 낮음`, `Unity 코드 제어 높음`
- 비행 거점
  - `Remesh 중요`, `Texture 중요`, `Animate 낮음`, `Unity 코드 제어 높음`

### 22. 집에서 작업할 때 가장 추천하는 실제 루프
1. 프롬프트로 4개 생성
2. 가장 좋은 1개 선택
3. Remesh로 정리
4. Texture로 진영 재질 입히기
5. Texture Edit로 이상한 부분만 보정
6. 인간형이면 Animate까지 진행
7. `FBX` 다운로드
8. Blender에서 필요하면 정리
9. Unity에 넣고 테스트
10. 이상하면 처음부터 다시 만들지 말고 어느 단계에서 망가졌는지 찾아 그 단계만 되돌린다


## 5. 기능 역할 요약

이 섹션은 `Remesh`, `Texture`, `Animate`가 각각 정확히 무슨 역할인지 헷갈리지 않게 정리한 것이다.
집에서 작업할 때는 아래 역할 구분만 명확히 기억해도 작업 순서가 크게 덜 꼬인다.

### 한 줄 요약
- `Remesh`는 `모델의 형태를 이루는 메시 구조를 정리하는 기능`이다.
- `Texture`는 `모델 표면의 색과 재질 느낌을 입히는 기능`이다.
- `Animate`는 `모델에 뼈대를 넣고 움직임을 붙이는 기능`이다.

### 가장 중요한 구분
- `Remesh`는 주로 `형태 구조`, `폴리 수`, `위상`을 바꾼다.
- `Texture`는 주로 `색`, `재질`, `표면 표현`을 바꾼다.
- `Animate`는 주로 `본`, `리깅`, `동작 클립`을 만든다.

즉:
- 모델이 너무 무겁거나 구조가 지저분하면 `Remesh`
- 색과 재질이 마음에 안 들면 `Texture`
- 사람이 걷고 뛰고 공격하게 만들고 싶으면 `Animate`

### 1. Remesh의 역할

#### Remesh는 무엇을 하는 기능인가
- 생성된 3D 모델의 `메시 구조`를 다시 정리한다.
- 폴리 수를 줄이거나 늘릴 수 있다.
- `사각형 면(Quad)` 또는 `삼각형 면(Triangle)` 같은 위상을 고를 수 있다.
- 게임 엔진, Blender 수정, 최적화, 포맷 다운로드에 더 맞는 형태로 메시를 바꿔 준다.

#### Remesh가 바꾸는 것
- 폴리 수
- 면 구조
- 메시 밀도
- 일부 형태 디테일 보존 방식
- 결과적으로 Unity에서 다루기 쉬운 정도

#### Remesh가 직접 바꾸지 않는 것
- 기본 색감
- 재질 느낌
- 망토 색, 갑옷 색, 발광부 색 같은 표면 표현
- 걷기, 공격, 죽음 같은 애니메이션

즉 `Remesh`는 `겉모습을 칠하는 기능`이 아니라 `몸통 구조를 다시 짜는 기능`에 가깝다.

#### Remesh를 왜 먼저 하는가
Meshy 공식 도움말 기준으로 `Remesh를 텍스처 전에 하는 것이 보통 더 좋은 UV 정렬 결과`를 만든다.
그래서 이 프로젝트에서는 `생성 -> 결과 선택 -> Remesh -> Texture` 순서를 기본으로 잡는다.

#### Remesh가 특히 중요한 경우
- 인간형 보병을 Blender에서 조금 더 정리하고 싶을 때
- 모델이 너무 무거워서 Unity에서 쓰기 부담스러울 때
- 포병, 전투기, 거점처럼 코드 제어할 병기형 모델을 단순하게 만들고 싶을 때
- 리깅 전에 폴리 수를 줄여야 할 때

#### Remesh를 너무 믿으면 안 되는 경우
- 메시가 단순해졌다고 해서 자동으로 예뻐지는 것은 아니다.
- 폴리 수를 너무 낮추면 손, 무기, 망토, 날개 끝 같은 형태가 쉽게 무너진다.
- 특히 인간형은 너무 공격적으로 줄이면 리깅 품질도 나빠질 수 있다.

### 2. Texture의 역할

#### Texture는 무엇을 하는 기능인가
- 이미 만들어진 모델 표면에 `색`, `재질`, `표면 분위기`를 입히거나 업데이트한다.
- 무텍스처 모델에도 쓸 수 있고, 이미 텍스처가 있는 모델에도 다시 적용할 수 있다.
- 텍스트 프롬프트나 이미지 입력으로 스타일을 바꿀 수 있다.

#### Texture가 바꾸는 것
- 갑옷 색
- 금속, 세라믹, 천 같은 재질 인상
- 발광부 분위기
- 오염, 마모, 전투 손상 느낌
- 진영 통일감

#### Texture가 직접 바꾸지 않는 것
- 캐릭터 몸 비율
- 무기 길이
- 포신 구조
- 실루엣 자체
- 걷기/공격/죽음 동작

즉 `Texture`는 `형태를 고치는 기능`이 아니라 `이미 있는 형태 위에 표면 느낌을 입히는 기능`이다.

#### Texture를 언제 쓰나
- 형태가 확정된 뒤 진영 색과 재질을 입히고 싶을 때
- 같은 모델을 플레이어 진영/적 진영 스타일로 나눠 보고 싶을 때
- 무텍스처 결과가 너무 밋밋할 때
- 전체적인 시각 톤을 빠르게 잡고 싶을 때

#### Texture를 먼저 쓰면 안 좋은 이유
- 아직 형태가 불안정한데 텍스처부터 얹으면, 나중에 Remesh를 다시 하거나 모델을 갈아엎을 가능성이 높다.
- 그래서 형태가 대충이라도 확정되기 전에는 Texture에 과하게 시간 쓰지 않는 편이 좋다.

#### Texture와 Texture Edit의 차이
- `Texture`는 모델 전체 표면을 한 번에 다시 입히는 쪽에 가깝다.
- `Texture Edit`는 이미 괜찮은 결과에서 특정 부위만 고치는 기능이다.

예를 들면:
- 갑옷 전체 톤을 바꾸고 싶다 -> `Texture`
- 방패만 더 밝게 칠하고 싶다 -> `Texture Edit`
- 망토만 붉게 바꾸고 싶다 -> `Texture Edit`

### 3. Animate의 역할

#### Animate는 무엇을 하는 기능인가
- 모델에 뼈대(armature, skeleton)를 넣는 `Rigging`을 수행한다.
- 그 뼈대를 바탕으로 `걷기`, `달리기`, `공격`, `죽음` 같은 애니메이션을 붙인다.
- 결과적으로 Unity Animator나 Blender에서 쓸 수 있는 움직이는 캐릭터 자산으로 바꿔 준다.

#### Animate가 바꾸는 것
- 본 구조
- 스킨 바인딩
- 애니메이션 클립
- 걷기/공격/죽음 같은 재생 가능한 동작 세트

#### Animate가 직접 바꾸지 않는 것
- 갑옷 색
- 금속 질감
- 폴리 수 최적화
- 포병/전투기/거점의 구조적 단순화

즉 `Animate`는 `색 입히기`나 `메시 정리`가 아니라 `움직일 준비를 시키는 기능`이다.

#### Animate가 잘 맞는 대상
- 팔과 다리가 분명한 인간형 유닛
- T-Pose 또는 A-Pose로 생성된 보병 캐릭터
- Unity에서 `Idle / Move / Attack / Death`를 붙일 대상

#### Animate가 잘 안 맞는 대상
- 포병
- 전투기
- 이동 거점
- 비행 거점
- 팔다리 구조가 불분명한 비인간형 자산

Meshy 공식 API 문서도 현재 자동 리깅/애니메이션은 `standard humanoid (bipedal)`에 가장 잘 맞는다고 설명한다.
그래서 이 프로젝트에서는 `인간형 = Animate 활용`, `병기형 = Unity 코드 제어`를 기본 원칙으로 둔다.

### 4. 세 기능의 관계를 한 번에 이해하기

#### 예시 1. 창병을 만들 때
- 창병의 몸체가 너무 무겁고 지저분하다 -> `Remesh`
- 창병 갑옷을 상아색+황동으로 바꾸고 싶다 -> `Texture`
- 창병이 걷고 공격하게 만들고 싶다 -> `Animate`

#### 예시 2. 포병을 만들 때
- 포신과 차체를 더 가볍게 정리하고 싶다 -> `Remesh`
- 어두운 금속과 주황 발광부를 넣고 싶다 -> `Texture`
- 포병을 움직이게 하고 싶다 -> `Animate`보다 `Unity 코드 제어`

#### 예시 3. 전투기를 만들 때
- 날개와 동체 구조를 가볍게 정리하고 싶다 -> `Remesh`
- 기체 패널 색과 문양을 넣고 싶다 -> `Texture`
- 비행 동작을 만들고 싶다 -> `Animate`보다 `Unity 코드 제어`

### 5. 이 프로젝트 기준 기능 사용 순서

#### 인간형 유닛
1. `Text to 3D`로 생성
2. 가장 좋은 결과 선택
3. `Remesh`
4. `Texture`
5. 필요하면 `Texture Edit`
6. `Animate`의 `Rigging`
7. 필요한 애니메이션 추가
8. `FBX` 다운로드
9. Unity 반입

#### 비인간형 유닛
1. `Text to 3D`로 생성
2. 가장 좋은 결과 선택
3. `Remesh`
4. `Texture`
5. 필요하면 `Texture Edit`
6. `FBX` 다운로드
7. Unity에서 코드로 움직임 구현

### 6. 지금 보이는 Remesh 창 기준 빠른 선택 요약
이 부분은 바로 위에서 정리한 리메시 UI 선택 내용을 빠르게 다시 보는 용도다.

- 인간형 기본 보병
  - `고정형 + 10K + 사각형 면`
- 인간형 엘리트
  - `고정형 + 30K + 사각형 면`
- 포병
  - `고정형 + 10K + 삼각형 면`
- 전투기
  - `고정형 + 10K + 삼각형 면`
- 이동 거점 / 비행 거점
  - `고정형 + 30K + 삼각형 면`

형태가 무너지면 다음 단계로 올린다.
- 기본 보병: `10K -> 30K`
- 포병/전투기: `10K -> 30K`
- 거점류: `30K -> 100K`

### 7. 헷갈릴 때 바로 보는 판단표
- `모델 구조가 문제다` -> `Remesh`
- `색과 재질이 문제다` -> `Texture`
- `일부 부위 색만 문제다` -> `Texture Edit`
- `사람처럼 움직여야 한다` -> `Animate`
- `병기나 비행체를 움직여야 한다` -> `Unity 코드 제어`

### 8. 이 문단의 내용 중 공식 기준과 프로젝트 기준
- `Remesh를 Texture 전에 하는 것이 일반적으로 좋다`는 점은 Meshy 공식 도움말 기준이다.
- `Texture`가 색/재질을 바꾸고, `Animate`가 인간형 리깅과 동작에 적합하다는 설명도 Meshy 공식 도움말/API 기준이다.
- `고정형 + 10K + 사각형 면`, `고정형 + 30K + 삼각형 면` 같은 구체 추천값은 이 프로젝트용 운영 기준으로 내가 정리한 실전 추천값이다.

### 9. Rigging의 역할

#### Rigging은 정확히 어떤 역할인가
- `Rigging`은 모델 안에 `움직임용 뼈대`를 넣는 단계다.
- 쉽게 말하면, 캐릭터 안에 `머리`, `척추`, `팔`, `다리`, `손`, `발` 같은 움직임 기준점을 심는 작업이다.
- 이 뼈대가 있어야 팔을 들고, 다리를 움직이고, 몸을 숙이고, 공격 자세를 만들 수 있다.
- 뼈대만 있다고 바로 걷는 것은 아니고, `애니메이션을 적용할 준비가 된 상태`가 되는 것이다.

#### Rigging이 실제로 하는 일
- 모델의 몸 구조를 분석한다.
- 내부에 본(bones) 또는 armature를 만든다.
- 메시 표면이 어떤 본을 따라 움직일지 연결한다. 이것을 보통 `skin binding`이라고 본다.
- 이후 `Walk`, `Run`, `Attack` 같은 애니메이션이 들어오면, 그 본들이 움직이면서 모델도 같이 움직이게 된다.

#### Rigging이 없으면 안 되는 이유
- 리깅이 없으면 캐릭터는 `조각상`처럼 서 있기만 한다.
- 애니메이션 클립이 있어도, 어떤 부위를 어떻게 움직일지 기준이 없어서 자연스럽게 움직일 수 없다.
- Unity에서 Animator를 써도, 기본적으로는 본 구조가 있는 캐릭터가 훨씬 다루기 쉽다.

#### Rigging과 Animation의 차이
- `Rigging`
  - 움직일 수 있는 몸 구조를 만든다.
  - 본, 관절, 스킨 연결이 핵심이다.
- `Animation`
  - 이미 만들어진 몸 구조를 실제로 움직인다.
  - 걷기, 달리기, 공격, 죽음 같은 동작 클립이 핵심이다.

한 줄로 구분하면:
- `Rigging = 몸의 골격 준비`
- `Animation = 그 골격을 실제로 움직이는 동작`

#### 이 프로젝트에서 Rigging이 중요한 이유
- 창병, 방패 보병, 총병, 특수전사, 친위대 같은 인간형 유닛은 모두 `걷기`, `공격`, `죽음`이 필요하다.
- 이런 유닛은 리깅이 잘못되면 팔이 꺾이거나, 무기가 손에서 뜨거나, 다리가 비정상적으로 휘는 문제가 생긴다.
- 그래서 인간형은 `모델 예쁨`만 보지 말고 `리깅이 잘 붙는 구조인가`도 같이 봐야 한다.

#### Rigging이 잘 맞는 모델
- 팔다리가 분명한 인간형
- T-Pose 또는 A-Pose에 가까운 캐릭터
- 몸통, 팔, 다리 구분이 뚜렷한 보병형 캐릭터
- 무기가 몸에 너무 묻혀 있지 않은 캐릭터

#### Rigging이 잘 안 맞는 모델
- 포병, 전투기, 이동 거점, 비행 거점 같은 비인간형 병기
- 팔다리 구분이 애매한 모델
- 몸과 망토, 무기, 장식이 지나치게 한 덩어리처럼 뭉친 모델
- 팔과 다리 위치가 비정상적으로 꼬인 모델
- 폴리 수가 너무 많거나 구조가 지나치게 복잡한 모델

Meshy 공식 API 문서 기준으로도 자동 리깅은 현재 `standard humanoid (bipedal)`에 가장 잘 맞고, `non-humanoid`나 `unclear limb and body structure`에는 적합하지 않다.
또한 얼굴 수가 너무 많으면 리깅 전 `Remesh`로 줄이는 것이 권장된다.

#### Rigging 결과를 볼 때 체크할 것
1. 어깨가 지나치게 올라가거나 찌그러지지 않는가
2. 팔꿈치와 무릎이 이상한 방향으로 꺾이지 않는가
3. 손이 무기에서 너무 멀어지지 않는가
4. 다리가 걷기 자세에서 서로 심하게 겹치지 않는가
5. 망토나 긴 천이 몸을 과하게 관통하지 않는가
6. 몸통이 회전할 때 흉갑과 허리가 비정상적으로 찢어지지 않는가

#### Rigging이 애매하면 어떻게 판단하나
- 대기 자세에서 멀쩡한데 걷기에서 무너지면 `리깅/애니메이션 궁합` 문제일 가능성이 크다.
- 공격 자세에서 손과 무기가 심하게 분리되면 해당 모델은 기준 캐릭터로 쓰기 어렵다.
- 팔, 다리, 어깨가 계속 찌그러지면 Remesh를 다시 하거나 다른 생성 결과를 고르는 편이 낫다.
- 즉 리깅이 이상할 때는 억지로 쓰기보다 `생성 결과 선택 단계`로 한 번 돌아가는 편이 안전하다.

#### 이 프로젝트 기준 Rigging 사용 원칙
- 인간형 유닛은 `Texture`까지 대체로 확정된 뒤 `Rigging`으로 간다.
- 비인간형 병기에는 기본적으로 Rigging을 먼저 고려하지 않는다.
- 인간형이라도 메시가 너무 무거우면 `Remesh` 후 Rigging으로 간다.
- Rigging이 잘 붙는 모델이 결국 Unity에서도 다루기 쉽다.


