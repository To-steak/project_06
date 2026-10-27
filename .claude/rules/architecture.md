---
paths:
  - "Project/Assets/Scripts/**/*.cs"
  - "Project/Assets/Scripts/**/*.asmdef"
  - "Project/Assets/Scripts/**/*.asmref"
---

# 아키텍처 규칙

ECS 패키지(DOTS)는 쓰지 않는다. "System은 함수, 상태는 데이터"라는 철학만 순수 C#으로 따른다.

## Logic / View 분리
- **Logic은 FixedUpdate에서만 실행한다.** 게임 상태를 바꾸는 코드는 System의 `Tick` 안에만 둔다.
- **View는 Update / LateUpdate에서만 실행한다.** 애니메이션, SFX, VFX, UI, 카메라가 View다.
- Logic은 View를 참조하지 않는다. 그 틱에 일어난 일은 프레임 기록 목록(`List<~Record>`)에 남긴다.
- View는 게임 상태를 읽기만 한다. 입력은 명령 버퍼(`~Command`)를 거친다.
- **Transform은 View만 쓴다.** Logic은 위치를 데이터(`previousPosition`, `currentPosition`)로 소유하고, View가 보간해 반영한다.
- 판정 시점은 Logic의 타이머가 정한다. 애니메이션 이벤트로 게임 상태를 바꾸지 않는다.
- Unity 물리(Rigidbody, Collider, `Physics` 쿼리)를 쓰지 않는다. 충돌과 범위 판정은 Logic이 직접 계산한다.

## World / Runner
- **World**는 한 판의 데이터(엔티티 저장소, 기록 목록, 요청 큐, 명령 버퍼, 난수 생성기)를 모아 두는 컨테이너다. Core에 있고 Runner만 쓴다. 판마다 `new World()`로 새로 만든다.
- **Runner가 World에서 필요한 데이터를 꺼내 System, ViewSystem, Presenter에 넘긴다.** 이들은 World를 받지 않는다.
- 틱 순서와 호출 순서는 Runner가 관리한다.
- DDOL에는 `Runner` 하나만 올린다. `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`에서 생성한다.

## System
- 로직은 `~System` 이름의 **static 클래스**로 만든다. **static 필드에 상태를 두지 않는다.**
- **`Tick`은 그 System이 읽고 쓰는 데이터만 인자로 받는다.** 시그니처가 곧 그 System의 영향 범위다. 읽기만 하는 struct는 `in`, 수정하는 struct는 `ref`로 받는다.

```csharp
// Runner
MoveSystem.Tick(_world.enemies, _world.enemyMovedRecords);
```

- **엔티티 생성은 SpawnSystem(틱 마지막)에서만 한다.** 다른 System은 생성 요청 큐(`Queue<~Request>`)에 넣기만 한다. 소멸은 즉시 하고, 이후 System은 `IsAlive`로 건너뛴다.
- 엔티티는 객체 참조 대신 `EntityIdentifier { index, generation }`로 가리킨다. 엔티티 저장소(`~Storage`)는 종류별 배열과 빈 슬롯 목록으로 보관하고, 슬롯을 재사용할 때 generation을 올린다.
- System 간 호출은 직접 호출한다.
- 서비스 클래스, ServiceLocator를 만들지 않는다.

## 데이터와 로딩
- 수치는 하드코딩하지 않고 `~Definition`(ScriptableObject)에 둔다.
- **데이터는 struct로 품는다.** Logic이 쓰는 값은 `Components/`의 `[Serializable]` struct(`~Component`)로 정의하고, `~Definition`과 `~Authoring`은 그 struct를 필드로 가진다. 같은 필드를 두 곳에 정의하지 않는다. 프리팹 등 View용 참조는 struct 밖에 둔다.
- **로딩:** Core가 `~Definition`의 struct를 World에 대입한다. `~Authoring`은 판 시작 시 Core가 struct에 Transform 위치 등 씬 값을 채워 World에 넣는다. System은 Definition과 Authoring을 참조하지 않는다.
- Definition은 `Assets/Resources/DefinitionCatalog.asset` 하나에 모은다. Runner가 시작할 때 `Resources.Load`로 이 Catalog만 읽는다. 다른 곳에서 `Resources.Load`를 쓰지 않는다.
- 테이블은 생성 시점에만 조회하고 필요한 값은 엔티티에 복사한다. 매 틱 로직은 엔티티 자신의 값만 쓴다.

## 이벤트 금지
- C# `event`, delegate 구독, 이벤트 버스, 이벤트 큐를 만들지 않는다.
- **예외: UI.** `Button.onClick` 등 UnityEvent와, `UserInterface/` 안에서 UI View가 발행해 Presenter가 구독하는 이벤트는 허용한다. 단 핸들러는 게임 상태를 직접 바꾸지 않고 명령 버퍼에 명령을 넣는 것까지만 한다.

## View
- Logic/View 분리는 코드 차원의 약속이며 어셈블리 참조로 지킨다. View 코드는 모두 `Presentation/` 어셈블리에 둔다.
- `Presentation/`은 영역별 하위 폴더로 나누고, 영역 폴더 안은 `ViewSystems/`(`~ViewSystem`)와 `Views/`(`~View` MonoBehaviour)로 나눈다. 여러 영역이 함께 쓰는 도구는 역할 이름의 하위 폴더에 둔다.

```
Presentation/
├── ViewContext.cs
├── Player/
│   ├── ViewSystems/    PlayerViewSystem
│   └── Views/          PlayerView
├── Enemy/
│   ├── ViewSystems/
│   └── Views/
├── Audio/              오디오 재생
├── Pooling/            오브젝트 풀
└── Camera/             카메라
```

- `Presentation/`의 `~View`는 인스펙터에서 연결한 참조를 public 필드로 가진다. 메서드를 두지 않는다.
- 연출의 판단과 동작은 모두 `~ViewSystem`(static)이 한다. Animator 파라미터 이름 등은 `~ViewSystem`의 상수로 둔다.
- View는 Runner가 LateUpdate에서 정해진 순서로 호출하는 `~ViewSystem.Present`가 처리한다. 인자는 System처럼 필요한 데이터와 ViewContext만 받는다.
- View 쪽 실행 상태(EntityIdentifier → View 매핑, 풀, 카메라 참조)는 **ViewContext 인스턴스**에 둔다. ViewSystem도 static 필드에 상태를 두지 않는다.
- 지속 상태는 매 프레임 엔티티 저장소를 읽고, 그 순간의 일은 프레임 기록을 읽는다. 프레임 기록은 struct이며 객체 참조를 담지 않는다. 기록 목록은 Runner가 LateUpdate 마지막에 비운다.
- View 오브젝트는 살아 있는 엔티티와 비교해 생성·반납한다. 게임 루프에서 `Instantiate` / `Destroy`를 반복하지 않고 풀을 쓴다.
- UI는 MVP 구조다. `UserInterface/`은 화면 영역별 하위 폴더로 나누고, 영역 폴더 안은 `Views/`(`~View`)와 `Presenters/`(`~Presenter`)로 나눈다.
- UI View는 버튼 입력을 이벤트로 발행하고 표시 메서드(`SetHealth` 등)를 가진다. Presenter는 MonoBehaviour이며, View의 이벤트를 구독해 명령 버퍼에 넣고, Runner에게 필요한 데이터만 받아 View의 표시 메서드를 부른다. Model 변경 알림은 없고 Runner가 매 프레임 데이터를 넘긴다.

## 입력
- 프레임 단위 입력(`wasPressedThisFrame` 등)은 Update에서만 읽는다.
- Runner가 Update에서 명령 버퍼에 쌓고, CommandSystem이 Tick에서 소비한다.

## 단위
- 시뮬레이션은 50Hz 고정(1틱 = 20ms). 시간 값은 **정수 틱**으로 다루고 필드명은 `~Ticks`로 끝낸다.
- 거리는 월드 유닛(1 = 1m), XZ 평면 기준. 거리 비교는 `sqrMagnitude`로 한다.
- 랜덤은 World가 가진 시드 고정 `System.Random`을 인자로 받아 쓴다. System에서 `UnityEngine.Random`을 쓰지 않는다.
- 매 틱 할당(LINQ, 클로저, 박싱, 문자열 결합)을 피한다.

## 폴더와 어셈블리
`Project/Assets/Scripts/` 아래에 폴더를 둔다. 같은 종류의 타입을 모은 폴더는 복수형, 역할을 나타내는 폴더는 단수형으로 짓는다. 어셈블리 이름은 `Project06.`으로 시작한다.

| 폴더 | 내용 | 어셈블리 | 참조 가능 |
|---|---|---|---|
| `Entities/` | `~Entity` struct, `EntityIdentifier`, `~Storage` | `Project06.Entity` (`.asmdef`) | 없음 |
| `Components/` | `~Component` struct | `Project06.Entity` (`.asmref`) | 〃 |
| `Records/` | `~Record` struct | `Project06.Entity` (`.asmref`) | 〃 |
| `Requests/` | `~Request` struct | `Project06.Entity` (`.asmref`) | 〃 |
| `Commands/` | `~Command` struct, `CommandBuffer` | `Project06.Entity` (`.asmref`) | 〃 |
| `Systems/` | static 로직 | `Project06.Systems` | Entity |
| `Definitions/` | `~Definition` (ScriptableObject 데이터) | `Project06.Definitions` | Entity |
| `Authoring/` | `~Authoring` (씬 배치 MonoBehaviour) | `Project06.Authoring` | Entity |
| `Persistence/` | 설정·기록 저장과 불러오기 | `Project06.Persistence` | Entity |
| `Presentation/` | `~ViewSystem`, `~View`, ViewContext, 오디오, 오브젝트 풀 | `Project06.Presentation` | Entity |
| `UserInterface/` | UI View, Presenter | `Project06.UserInterface` | Entity, Persistence |
| `Core/` | World, Runner, 부트스트랩, 로딩, 씬 전환 | `Project06.Core` | 전부 |
| `Editor/` | 에디터 도구 | `Project06.Editor` (플랫폼: Editor 전용) | Definitions, Authoring |
