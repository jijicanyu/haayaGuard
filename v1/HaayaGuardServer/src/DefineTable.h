/*
 * DefineTable.h
 *
 *  Created on: Dec 24, 2014
 *      Author: wind
 */

#ifndef DEFINETABLE_H_
#define DEFINETABLE_H_
#include <string>;
using std::string;
class DefineTable {
public:
	DefineTable();
	virtual ~DefineTable();
public:
	static const string SendImage="cn";
	static const string OtherClientConnected="co";
	static const string OpenHole="c";
	static const string Passed="p";
};

#endif /* DEFINETABLE_H_ */
