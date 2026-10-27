---
paths:
  - "Project/Assets/Scripts/**/*.cs"
---

# C# 코드 규칙

## 네임스페이스
- `Project06.*`, 폴더 경로와 일치시킨다. 예: `Component/Entity/` → `Project06.Component.Entity`.
- `Project06.System` 때문에 본문의 `System.~`은 .NET이 아니라 `Project06.System`으로 해석된다. .NET 타입은 파일 맨 위 `using`으로 가져오고, 본문에서 정규화된 이름이 필요하면 `global::System.~`로 쓴다.

## 이름
- 단어를 줄여 쓰지 않는다 (`cnt`, `idx`, `pos` 금지). 반복 변수도 `i`, `j` 대신 `index`, `enemyIndex`. 외부 API 이름은 예외.
- 머리글자 약어도 쓰지 않는다 (`HP` 대신 `Health`, `ID` 대신 `Identifier`, `UI` 대신 `UserInterface`).

| 대상 | 규칙 | 예시 |
|---|---|---|
| 타입, 메서드, 프로퍼티 | PascalCase | `MoveSystem`, `Tick()` |
| public 필드 | camelCase | `currentPosition` |
| private 필드 | `_camelCase` | `_runner` |
| 지역 변수, 매개변수 | camelCase | `targetEnemy` |
| 상수 | UPPER_SNAKE_CASE | `MAX_HEALTH` |
| bool | `is`, `has`, `can` 접두사 | `isAlive` |

접미사: `~System`, `~ViewSystem`, `~Record`, `~Request`, `~Data`, `~Definition`, `~Authoring`, `~View`, `~Presenter`, 시간 값은 `~Ticks`.

## 스타일
- `var` 금지. 타입을 명시한다.
- 파일 하나에 타입 하나, 파일 이름은 타입 이름과 같다. 중첩 타입 금지.
- 중괄호는 다음 줄에 연다 (Allman).
- 멤버 순서: public 필드 → `[SerializeField] private` 필드 → private 필드 → const → public 메서드 → private 메서드.
- 엔티티 데이터는 struct + public 필드. 배열 원소 struct는 **반드시 `ref`로 접근**한다 (`ref Enemy enemy = ref world.enemies[index];`). 매개변수는 수정하면 `ref`, 읽기만 하면 `in`.

## Unity
- MonoBehaviour와 ScriptableObject는 파일 이름과 클래스 이름이 같아야 한다.
- 직렬화 필드 이름을 바꿀 때는 `[FormerlySerializedAs("이전이름")]`을 붙인다. 붙이지 않으면 에디터에서 입력한 값이 사라진다.
- 입력은 새 Input System만 쓴다. 구 `UnityEngine.Input` API를 쓰지 않는다.

## 주석
- public 메서드: 한국어 XML 문서 주석. `<summary>` 필수, 매개변수가 있으면 `<param>`, 반환값이 있으면 `<returns>`.
- public 필드: 선언 바로 윗줄에 한국어 한 줄 주석.
- private 멤버와 지역 변수는 필요할 때만 단다.
