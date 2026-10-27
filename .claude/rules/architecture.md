---
paths:
  - "Project/Assets/Scripts/**/*.cs"
---

# 아키텍처 규칙

ECS 패키지(DOTS)는 쓰지 않는다. "System은 함수, 상태는 데이터"라는 철학만 순수 C#으로 따른다.

## Logic / View 분리
- **Logic은 FixedUpdate에서만 실행한다.** 게임 상태를 바꾸는 코드는 System의 `Tick(World)` 안에만 둔다.
- **View는 Update / LateUpdate에서만 실행한다.** 애니메이션, SFX, VFX, UI, 카메라가 View다.
- Logic은 View를 참조하지 않는다. 그 틱에 일어난 일은 World의 프레임 기록 목록(`List<~Record>`)에 남긴다.
- View는 World를 읽기만 한다. 입력은 `world.Input` 버퍼를 거친다.
- **Transform은 View만 쓴다.** Logic은 위치를 데이터(`previousPosition`, `currentPosition`)로 소유하고, View가 보간해 반영한다.
- 판정 시점은 Logic의 타이머가 정한다. 애니메이션 이벤트로 게임 상태를 바꾸지 않는다.

## World / System
- 한 판의 모든 상태는 **World**에 둔다. 판마다 `new World()`로 새로 만든다.
- 로직은 `~System` 이름의 **static 클래스**로 만든다. 시그니처는 `public static void Tick(World world)`. **static 필드에 상태를 두지 않는다.**
- **엔티티 생성은 SpawnSystem(틱 마지막)에서만 한다.** 다른 System은 World의 생성 요청 큐(`Queue<~Request>`)에 넣기만 한다. 소멸은 즉시 하고, 이후 System은 `IsAlive`로 건너뛴다.
- 엔티티는 객체 참조 대신 `EntityID { index, generation }`로 가리킨다. World는 종류별 배열과 빈 슬롯 목록으로 보관하고, 슬롯을 재사용할 때 generation을 올린다.
- System 간 호출은 직접 호출한다. 틱 순서는 Runner가 관리한다.
- 수치는 하드코딩하지 않고 `~Definition`(ScriptableObject)에 둔다. 테이블은 생성 시점에만 조회하고 필요한 값은 엔티티에 복사한다.
- 서비스 클래스, ServiceLocator를 만들지 않는다.

## App / Runner
- 프로그램 전체 수명의 기능은 **static `App`**에 둔다. App은 Foundation 어셈블리에 있다.
- **System은 App을 쓰지 않는다. World만 본다.** App은 View, UI, Core만 쓴다.
- DDOL에는 `Runner` 하나만 올린다. `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`에서 생성한다.
- **베이킹:** `~Definition`과 씬의 `~Authoring`은 Core에서 Component의 struct로 변환한다. System은 Authoring을 참조하지 않는다.

## 이벤트 금지
- C# `event`, delegate 구독, 이벤트 버스, 이벤트 큐를 만들지 않는다.
- **예외: UI.** `Button.onClick` 등 UnityEvent는 허용한다. 단 핸들러는 `world.Input`에 명령을 넣는 것까지만 한다.

## View
- View는 Runner가 LateUpdate에서 정해진 순서로 호출하는 `~ViewSystem.Present(world, viewContext)`가 처리한다.
- View 쪽 실행 상태(EntityID → View 매핑, 풀, 카메라 참조)는 **ViewContext 인스턴스**에 둔다. ViewSystem도 static 필드에 상태를 두지 않는다.
- 지속 상태는 매 프레임 World를 읽고, 그 순간의 일은 프레임 기록을 읽는다. 프레임 기록은 struct이며 객체 참조를 담지 않는다. 기록 목록은 Runner가 LateUpdate 마지막에 비운다.
- View 오브젝트는 World의 살아 있는 엔티티와 비교해 생성·반납한다. 게임 루프에서 `Instantiate` / `Destroy`를 반복하지 않고 풀을 쓴다.
- UI는 View-Presenter 구조다. Presenter가 World를 읽어 View를 갱신하고, 버튼 입력은 Presenter를 거쳐 `world.Input`에 들어간다.

## 입력
- 프레임 단위 입력(`wasPressedThisFrame` 등)은 Update에서만 읽는다.
- Runner가 Update에서 `world.Input`에 쌓고, CommandSystem이 Tick에서 소비한다.

## 단위
- 시뮬레이션은 50Hz 고정(1틱 = 20ms). 시간 값은 **정수 틱**으로 다루고 필드명은 `~Ticks`로 끝낸다.
- 거리는 월드 유닛(1 = 1m), XZ 평면 기준. 거리 비교는 `sqrMagnitude`로 한다.
- 랜덤은 World가 가진 시드 고정 `global::System.Random`을 쓴다. System에서 `UnityEngine.Random`을 쓰지 않는다.
- 매 틱 할당(LINQ, 클로저, 박싱, 문자열 결합)을 피한다.

## 폴더와 어셈블리
`Project/Assets/Scripts/` 아래 폴더마다 Assembly Definition을 두어 참조 방향을 강제한다.

| 폴더 | 내용 | 참조 가능 |
|---|---|---|
| `Component/` | World, 엔티티·기록·요청 struct | 없음 |
| `System/` | static 로직 | Component |
| `Authoring/` | `~Definition`, `~Authoring` | Component |
| `Foundation/` | static App과 그 기능 | Component |
| `View/` | `~ViewSystem`, `~View` | Component, Foundation |
| `UI/` | UI View, Presenter | Component, Foundation |
| `Core/` | Runner, 부트스트랩, 베이킹 | 전부 |
| `Editor/` | 에디터 도구 | Authoring |
