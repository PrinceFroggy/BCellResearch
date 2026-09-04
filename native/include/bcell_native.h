#pragma once
#if defined(_WIN32)
  #define BCELL_API __declspec(dllexport)
#else
  #define BCELL_API __attribute__((visibility("default")))
#endif
#ifdef __cplusplus
extern "C" {
#endif
BCELL_API double score_clone(const char* antigen, int memory_cell, double confidence);
#ifdef __cplusplus
}
#endif
