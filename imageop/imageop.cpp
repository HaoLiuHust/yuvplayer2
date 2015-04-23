#include "imageop.h"
using cv::Mat;
using cv::Range;
int mergeframe(BYTE *frameL, BYTE *frameR, BYTE *dstframe, int width, int height)
{
	cv::Mat FL(height, width, CV_8UC1, frameL);
	cv::Mat FR(height, width, CV_8UC1, frameR);

	cv::Mat FM(height, width << 1, CV_8UC1, dstframe);
	Mat flag1 = FM(Range::all(), Range(0, width));
	Mat flag2 = FM(Range::all(), Range(width, width << 1));
	FL.copyTo(flag1);
	FR.copyTo(flag2);
	return 1;
}

 

 int MergeAndConv(BYTE *frameL, BYTE *frameR, BYTE *dstframe, int m_width, int m_height)
 {
	 BYTE* YUVBUF[4], *RGBBUF[4];
	 int inlinesize[4], outlinesize[4];
	 BYTE *Y1, *Y2;
	 BYTE *U1, *U2;
	 BYTE *V1, *V2;
	 int picsize = m_width*m_height;
	 SwsContext *img_swscontext;
	 img_swscontext = sws_getContext(m_width << 1, m_height, PIX_FMT_YUV420P, m_width << 1, m_height, PIX_FMT_BGR24, SWS_POINT, NULL, NULL, NULL);
	 YUVBUF[0] = (BYTE*)malloc(picsize << 1);
	 YUVBUF[1] = (BYTE*)malloc(picsize >> 1);
	 YUVBUF[2] = (BYTE*)malloc(picsize >> 1);
	 YUVBUF[3] = NULL;

	 RGBBUF[0] = (BYTE*)malloc(picsize * 3 << 1);
	 RGBBUF[1] = NULL;
	 RGBBUF[2] = NULL;
	 RGBBUF[3] = NULL;

	 inlinesize[0] = m_width << 1;
	 inlinesize[1] = m_width;
	 inlinesize[2] = m_width;
	 inlinesize[3] = 0;

	 outlinesize[0] = m_width * 3 << 1;
	 outlinesize[1] = 0;
	 outlinesize[2] = 0;
	 outlinesize[3] = 0;
	 
	
	 Y1 = frameL;
	 U1 = frameL + picsize;
	 V1 = frameL + picsize + (picsize >> 2);
	 Y2 = frameR;
	 U2 = frameR + picsize;
	 V2 = frameR + picsize + (picsize >> 2);
	 mergeframe(Y1, Y2, YUVBUF[0], m_width, m_height);
	 mergeframe(U1, U2, YUVBUF[1], m_width >> 1, m_height >> 1);
	 mergeframe(V1, V2, YUVBUF[2], m_width >> 1, m_height >> 1);
	 sws_scale(img_swscontext, YUVBUF, inlinesize, 0, m_height, RGBBUF, outlinesize);

	 memcpy(dstframe, RGBBUF[0], picsize * 3 << 1);
	 for (int i = 0; i < 4; ++i)
	 {
		 free(YUVBUF[i]);
		 free(RGBBUF[i]);
	 }
	 return 0;
 }