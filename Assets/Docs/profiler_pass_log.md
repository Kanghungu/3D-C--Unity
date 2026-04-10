# Profiler 패스 실행 기록 (로컬)

절차는 [`PROFILER_PASS_TEMPLATE.md`](PROFILER_PASS_TEMPLATE.md) 를 따릅니다. **Unity 에디터에서 측정한 뒤** 아래에 한 블록씩 추가합니다(저장소에 숫자를 추측으로 채우지 않음).

---

## 템플릿 (복붙 후 채우기)

```
날짜: YYYY-MM-DD
Unity: (ProjectVersion.txt와 동일)
환경: Editor Play / Development Standalone
평균 CPU ms(대략): 
GC Alloc 눈에 띄는 프레임: 예/아니오
스파이크 상위 1~2: 
OnGUI 과다 여부(HUD 열림/닫힘 비교): 
결론: 이번 주 조치 없음 / (파일·함수명)
```

---

## 기록

2026-04-10 | 저장소 워크플로 반영용 플레이스홀더 — 로컬에서 NewSampleScene 한 판 Profiler Record 후 위 템플릿으로 실측 줄을 이 아래에 추가하세요.
