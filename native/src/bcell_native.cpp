#include "bcell_native.h"
#include <algorithm>
#include <cctype>
#include <string>

static std::string lower(std::string s) {
    std::transform(s.begin(), s.end(), s.begin(), [](unsigned char c){ return (char)std::tolower(c); });
    return s;
}

extern "C" BCELL_API double score_clone(const char* antigen, int memory_cell, double confidence) {
    const std::string a = lower(antigen ? antigen : "");
    double score = memory_cell ? 0.20 : 0.0;
    if (a.find("sars-cov-2") != std::string::npos || a.find("covid") != std::string::npos) score += 0.55;
    if (a.find("spike") != std::string::npos || a.find("rbd") != std::string::npos || a.find("nucleocapsid") != std::string::npos) score += 0.15;
    score += 0.10 * std::clamp(confidence, 0.0, 1.0);
    return std::clamp(score, 0.0, 1.0);
}
