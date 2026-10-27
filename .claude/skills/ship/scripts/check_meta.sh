#!/usr/bin/env bash
# Unity Assets 폴더의 변경 사항에서 .meta 짝이 맞지 않는 항목을 출력한다.
# 출력이 없으면 정상. 저장소 루트에서 실행한다.
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

# 경로가 Assets 폴더 안에 있는지 (Assets 폴더 자체는 .meta가 없다)
is_in_assets() {
    case "/$1" in
        */Assets/*) return 0 ;;
        *) return 1 ;;
    esac
}

declare -A reported=()
report() {
    if [[ -z "${reported[$1]:-}" ]]; then
        reported[$1]=1
        echo "$1"
    fi
}

# 새로 생긴 경로의 상위 폴더들도 .meta가 있어야 한다
check_parents() {
    local directory
    directory="$(dirname "$1")"
    while is_in_assets "$directory" && [[ "$(basename "$directory")" != "Assets" ]]; do
        if [[ ! -e "$directory.meta" ]]; then
            report "누락: $directory.meta (폴더)"
        fi
        directory="$(dirname "$directory")"
    done
}

while IFS= read -r -d '' entry; do
    status="${entry:0:2}"
    path="${entry:3}"
    # 이름 변경(R)은 다음 항목이 원래 경로다. 원래 경로의 .meta가 남아 있지 않은지도 본다.
    if [[ "$status" == R* || "$status" == ?R ]]; then
        IFS= read -r -d '' original || true
        if [[ "$original" != *.meta && ! -e "$original" && -e "$original.meta" ]] && is_in_assets "$original"; then
            report "삭제된 에셋의 .meta가 남음: $original.meta"
        fi
    fi
    is_in_assets "$path" || continue

    if [[ "$path" == *.meta ]]; then
        asset="${path%.meta}"
        if [[ -e "$path" && ! -e "$asset" ]]; then
            report "짝 없는 .meta: $path"
        fi
        continue
    fi

    if [[ -e "$path" ]]; then
        [[ -e "$path.meta" ]] || report "누락: $path.meta"
        check_parents "$path"
    elif [[ -e "$path.meta" ]]; then
        report "삭제된 에셋의 .meta가 남음: $path.meta"
    fi
done < <(git status --porcelain=v1 -z -uall)
