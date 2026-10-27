# CLAUDE.md

Unity 6 (6000.3.11f1), URP 프로젝트다. Unity 프로젝트 루트는 `Project/`, 스크립트는 `Project/Assets/Scripts/`에 둔다. 타겟 플랫폼은 미정이다.

## 대화
- 응답은 항상 한국어로 한다.
- 질문에는 답만 한다. 파일 수정은 작업을 요청받았을 때만 한다.
- 커밋·푸시·PR은 사용자가 `/ship`을 입력했을 때만 한다.
- 새 시스템이나 규칙을 도입해야 하면 코드 작성 전에 묻는다.
- 설계에 값이 없는 작은 수치나 동작은 정해서 진행하되, 응답에 무엇을 정했는지 알린다.

## 역할 분담
- Claude는 코드를 작성하고, Unity 에디터 작업(씬 배치, 인스펙터 연결, Animator, 프리팹)은 사용자가 한다.
- Claude는 에디터를 실행할 수 없다. 에디터에서 해야 할 일이 생기면 응답에 구체적으로 적는다.
- `.unity`, `.prefab`, `.asset` 파일을 직접 편집하지 않는다.

## Unity 주의
- **`.meta` 파일을 새로 만들지 않는다.** 새 에셋과 폴더의 `.meta`는 사용자가 에디터에서 생성해 커밋한다.
- 이미 있는 파일을 옮기거나 지울 때는 `.meta`도 함께 옮기거나 지운다. 옮길 때 `.meta`가 따라가지 않으면 GUID가 바뀌어 씬의 참조가 끊어진다.
- MonoBehaviour와 ScriptableObject는 파일 이름과 클래스 이름이 같아야 한다.
- 직렬화 필드 이름을 바꿀 때는 `[FormerlySerializedAs("이전이름")]`을 붙인다. 붙이지 않으면 에디터에서 입력한 값이 사라진다.
- `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`는 건드리지 않는다.
- 입력은 새 Input System만 쓴다. 구 `UnityEngine.Input` API를 쓰지 않는다.
