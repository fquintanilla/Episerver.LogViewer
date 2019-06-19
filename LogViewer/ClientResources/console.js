﻿var CS = CS || {};

CS.BufferLength = 200;
CS.Console = new Array();
CS.LastConsoleLength = 0;

CS.SetProgress = function (percent) {
	document.getElementById('bar').style.width = percent + '%';
	document.getElementById('percentage').innerHTML = percent + '%';
}

CS.SetStatus = function (status) {
	document.getElementById('status').innerHTML = status;
}

CS.WriteConsole = function (text) {
	CS.Console.push(text);
}

CS.ShowAll = function() {
	CS.BufferLength = Number.MAX_SAFE_INTEGER;
	CS.BatchComplete();
}

CS.BatchComplete = function () {
	if (CS.LastConsoleLength >= CS.Console.length) return;

	var bufferArray = CS.Console.slice(Math.max(0, CS.Console.length - CS.BufferLength), CS.Console.length);

	var contents = bufferArray.join('');

	if (bufferArray.length != CS.Console.length) {
		contents = "<button onclick=\"CS.ShowAll()\" class=\"expand\">Expand complete log</button>" + contents;
	}

	var consoleElement = document.getElementById('console');
	consoleElement.innerHTML = contents;

	var scrollHeight = Math.max(consoleElement.scrollHeight, consoleElement.clientHeight);
	consoleElement.scrollTop = scrollHeight - consoleElement.clientHeight;
}