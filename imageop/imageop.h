#ifndef IMAGEOP_H
#define IMAGEOP_H
#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include "cv.h"
extern "C"{
#include <libswscale/swscale.h>
#include <stdio.h>
}
typedef unsigned char BYTE;
extern "C" __declspec(dllexport) int  mergeframe(BYTE *frameL, BYTE *frameR, BYTE *dstframe, int width, int height);
extern "C" __declspec(dllexport) int  MergeAndConv(BYTE *frameL, BYTE *frameR, BYTE *dstframe, int width, int height);
#endif