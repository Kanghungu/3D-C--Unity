# Infantry Model Polish Guide

이 문서는 현재 [`create_player_infantry.py`](/f:/3D-Game-Develop/Assets/Art/Blender/create_player_infantry.py)로 만든 보병 모델을 Blender에서 더 자연스럽게 다듬기 위한 실전 가이드다.

목표는 세 가지다.

- 너무 각진 박스 느낌 줄이기
- 갑옷과 천이 조금 더 의도된 형태로 보이게 만들기
- 복잡한 리토폴로지 없이 빠르게 프로토타입 품질 올리기

이 문서는 `Bevel`, `Subdivision Surface`, `Edge Loop`만 써서 개선하는 방법만 다룬다.

## 1. 먼저 알아둘 기준

현재 모델은 크게 네 종류로 나뉜다.

- 박스형 파츠: 가슴 장식, 방패, 백팩, 허리 장식
- 원통형 파츠: 팔, 다리, 창대, 목
- 구형 파츠: 어깨, 팔꿈치, 무릎, 손 일부, 투구 일부
- 천 파츠: 타바드, 망토, 뒤 드레이프

각 파츠마다 손대는 방식이 다르다.

- 박스형 파츠: `Bevel`이 제일 효과적
- 원통형 파츠: `Edge Loop`로 굵기 변화
- 구형 파츠: `Subdivision Surface`를 아주 약하게
- 천 파츠: `Edge Loop` + 약한 `Bevel`

## 2. 작업 순서

추천 순서는 아래 순서다.

1. 큰 실루엣 수정
2. Bevel로 모서리 정리
3. Edge Loop로 형태 보강
4. 필요한 곳만 Subdivision
5. Shade Smooth와 Normal 확인

이 순서가 중요한 이유는 `Subdivision`을 먼저 넣으면 박스형 파츠가 쉽게 뭉개지기 때문이다.

## 3. Bevel 기본 원칙

### 언제 쓰면 좋은가

- 박스형 갑옷
- 방패 프레임
- 백팩
- 허리 가드
- 장화 앞코

### 언제 과하면 안 좋은가

- 아주 작은 장식 조각
- 얇은 천 조각
- 이미 둥근 구형 파츠

### 추천 세팅

프로토타입 기준 추천값:

- Width: `0.002 ~ 0.01`
- Segments: `2`
- Limit Method: `Angle`

현재 모델에선 보통 이 정도로 보면 된다.

- 작은 장식: `0.002 ~ 0.004`
- 중간 갑옷 파츠: `0.004 ~ 0.008`
- 큰 장화/방패/백팩: `0.008 ~ 0.012`

### Blender에서 적용 방법

1. 오브젝트 선택
2. 오른쪽 `Modifier Properties`
3. `Add Modifier`
4. `Bevel`
5. `Width`를 아주 조금만 올리기
6. `Segments`를 `2`로 설정

## 4. Subdivision Surface 기본 원칙

### 언제 쓰면 좋은가

- 머리/헬멧 코어
- 어깨 구체
- 무릎, 팔꿈치
- 손목이나 손처럼 약간 유기적인 부분

### 언제 조심해야 하나

- 방패
- 백팩
- 가슴 장식 패널
- 얇은 판 구조

이런 파츠는 Subdivision을 넣으면 각이 죽어서 장난감처럼 퍼질 수 있다.

### 추천 세팅

- Viewport Levels: `1`
- Render Levels: `1`

프로토타입에선 보통 `1`이면 충분하다.

### Blender에서 적용 방법

1. 오브젝트 선택
2. `Modifier Properties`
3. `Add Modifier`
4. `Subdivision Surface`
5. `Levels Viewport = 1`
6. 너무 뭉개지면 삭제하거나 Edge Loop를 먼저 추가

## 5. Edge Loop 기본 원칙

`Ctrl + R`로 넣는 Edge Loop는 형태를 가장 자연스럽게 보강하는 수단이다.

이건 크게 두 가지 용도로 쓴다.

- 형태를 더 또렷하게 유지
- 특정 부분만 좁히거나 넓히기

### 자주 쓰는 위치

- 장화 앞코
- 손목
- 창날 밑 Collar 부근
- 백팩 위아래 경계
- 망토 위쪽 고정 부분

### Blender에서 적용 방법

1. 오브젝트 선택
2. `Tab`으로 Edit Mode
3. `Ctrl + R`
4. 마우스를 가까운 면에 가져가 loop preview 확인
5. 클릭해서 loop 추가
6. 바로 슬라이드해서 위치 조정

## 6. 파츠별 추천 수정

## 6-1. 헬멧

현재 목표:

- 둥글지만 흐물거리지 않게
- 앞면 마스크와 뒷통수 덩어리가 명확히 나뉘게

추천 방법:

- `HelmetCore`, `HelmetBack`, `HelmetFrontShell`에는 `Subdivision Surface 1`
- `HelmetMask`, `HelmetBrowPlate`, `HelmetJawPlate`에는 약한 `Bevel`
- 헬멧 뒤쪽이 너무 풍선 같으면 세로 방향 edge loop 1개 추가

실전 팁:

- 헬멧은 전부 둥글게 만드는 것보다 `코어는 둥글고 장갑판은 각지게`가 더 자연스럽다

## 6-2. 어깨와 팔

현재 목표:

- 어깨 구체와 팔 원통이 자연스럽게 연결
- 차렷 느낌 줄이고 장비 든 느낌 강화

추천 방법:

- `Shoulder_*`, `Elbow_*`는 `Subdivision 1`
- `UpperArm_*`, `Forearm_*`는 loop 1~2개 추가
- 팔 중앙보다 관절 가까이에 loop를 넣어서 관절이 덜 밋밋하게 보이게

Edge Loop 위치 추천:

- 상완 윗부분 1개
- 팔꿈치 위 1개
- 손목 바로 위 1개

## 6-3. 손

현재 목표:

- 공처럼 보이지 않게
- 장갑/건틀릿 느낌 강화

추천 방법:

- `Hand_*`, `Thumb_*`는 `Subdivision 1`
- `HandGuard_*`, `KnuckleGuard_*`는 `Bevel`
- 손바닥 덩어리가 너무 둥글면 가로 loop를 1개 넣어서 납작하게 조절

실전 팁:

- 손은 완전 사실적으로 만들려 하지 말고 `손등`, `엄지`, `손목` 3덩어리만 읽혀도 훨씬 좋아진다

## 6-4. 가슴과 허리 갑옷

현재 목표:

- 패널이 너무 딱 붙은 벽돌처럼 안 보이게
- 중심 판, 옆판, 하단 판이 층으로 읽히게

추천 방법:

- `ChestUpper`, `ChestMid`, `ChestCenter`, `Fauld_*`, `WaistGuard`에 `Bevel`
- 큰 판에는 가장자리 안쪽으로 edge loop 1개 추가

이유:

- Subdivision 없이도 loop 하나만 안쪽에 있으면 모서리가 더 고급스럽게 유지된다

추천 순서:

1. Edge Loop로 판 가장자리 안쪽 지지선 만들기
2. 그 다음 약한 Bevel 적용

## 6-5. 백팩과 등판

현재 목표:

- 그냥 네모 상자 1개처럼 안 보이게
- 장비 프레임이 있는 등판처럼 보이게

추천 방법:

- `BackPack`, `BackPlate`, `BackPackTop`, `BackPackLower`에 `Bevel`
- 위아래 경계선용 edge loop 추가
- 배기구 프레임은 bevel만 유지

Edge Loop 추천 위치:

- 백팩 상단에서 15~20% 지점
- 백팩 하단에서 15~20% 지점

이 두 줄만 있어도 상자 느낌이 많이 줄어든다.

## 6-6. 장화

현재 목표:

- 발이 벽돌처럼 안 보이게
- 앞코, 발등, 발목이 나뉘게

추천 방법:

- `BootMain_*`, `BootToe_*`, `BootHeel_*`, `BootGuard_*`에 `Bevel`
- `BootMain_*`에 세로 loop 1개, 가로 loop 1개
- `BootToe_*`는 앞쪽으로 갈수록 조금 좁히기

추천 수정:

1. `BootMain` Edit Mode 진입
2. 발볼 근처 가로 loop 추가
3. 앞코 근처 세로 loop 추가
4. 앞쪽 면만 약간 줄여서 발끝 taper 만들기

## 6-7. 방패

현재 목표:

- 두꺼운 판과 프레임이 분리되어 보이게
- 너무 완전한 직육면체 느낌 줄이기

추천 방법:

- 본체와 프레임에 `Bevel`
- 바깥 프레임 안쪽에 edge loop 1개
- 중앙 보스는 `Subdivision 1`

실전 팁:

- 방패는 전체를 subdivision 하지 말고 `보스만 둥글게` 하는 편이 더 좋다

## 6-8. 창

현재 목표:

- 막대기 느낌 줄이기
- 창대, 그립, 창날, collar가 분리되어 보이게

추천 방법:

- `SpearShaft*`는 subdivision보다 굵기 변화가 더 중요
- 창대 상단/하단 경계에 edge loop 추가
- `SpearBladeCore`, `SpearCollar`, `SpearLug_*`는 약한 bevel
- `SpearTip`은 필요하면 subdivision 1

실전 팁:

- 창대는 너무 둥글게 다듬는 것보다 `중간은 곧고, 손잡이 위아래만 변화`가 있는 편이 더 무기답다

## 6-9. 천 파츠

현재 목표:

- 철판 같지 않게
- 위는 고정되고 아래는 떨어지는 느낌

추천 방법:

- `CapeUpper/Mid/Lower`, `TabardPanel_*`, `RearDrape*`는 bevel을 아주 약하게만 사용
- 위쪽 고정부 가까이에 edge loop 1개
- 아래쪽은 조금 더 넓고 단순하게 유지

실전 팁:

- 천에 subdivision을 바로 넣으면 판때기처럼 휘거나 이상하게 부풀 수 있다
- 먼저 edge loop와 비율 조정부터 하는 게 낫다

## 7. 초보 기준 추천 워크플로우

현재 모델을 빠르게 정리하려면 아래 순서대로 하면 된다.

1. 박스형 갑옷 파츠 전체에 `Bevel` 상태 먼저 통일
2. 장화, 백팩, 가슴 패널에 필요한 edge loop 추가
3. 헬멧, 어깨, 팔꿈치, 손만 `Subdivision 1`
4. `Shade Smooth` 적용
5. 너무 흐물거리는 파츠만 다시 loop 추가

## 8. 실수하기 쉬운 부분

### 실수 1. 모든 파츠에 subdivision 넣기

결과:

- 갑옷이 녹은 비누처럼 보임

해결:

- 구형 파츠에만 제한적으로 사용

### 실수 2. bevel width를 크게 주기

결과:

- 작은 장식이 뭉개짐

해결:

- 작은 파츠는 `0.002 ~ 0.004`부터 시작

### 실수 3. edge loop를 너무 많이 넣기

결과:

- 프로토타입인데 수정이 어려워짐

해결:

- 파츠당 1~2개만 먼저 넣고 확인

## 9. 가장 먼저 손볼 추천 우선순위

지금 모델 기준 우선순위는 아래가 좋다.

1. 장화 `BootMain`, `BootToe`
2. 손 `Hand`, `Thumb`, `HandGuard`
3. 백팩 `BackPack`, `BackPlate`
4. 가슴 패널 `ChestUpper`, `ChestMid`, `Fauld`
5. 헬멧 코어와 마스크
6. 천 파츠

## 10. 체크리스트

수정 후 아래 기준으로 보면 된다.

- 멀리서 봐도 손, 방패, 창이 분리되어 읽히는가
- 장화가 네모 블록이 아니라 발 형태로 보이는가
- 헬멧이 풍선 같지 않고 장갑판 느낌이 남아 있는가
- 백팩이 등에 붙은 장비처럼 보이는가
- 망토와 타바드가 철판이 아니라 천처럼 보이는가

## 11. 한 줄 결론

현재 모델은 `모든 걸 subdivision으로 해결`하려 하지 말고, `박스형은 bevel`, `형태 보강은 edge loop`, `둥근 파츠만 subdivision 1` 이 기준으로 다듬는 게 가장 자연스럽고 안전하다.
