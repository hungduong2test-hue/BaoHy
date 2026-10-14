function doPost(e) {
  var fields = e && e.parameter ? e.parameter : {};
  if (fields.website) {
    return ContentService.createTextOutput("Ignored");
  }

  var guest = cleanField(fields.guest, 120);
  var attending = cleanField(fields.attending, 100);
  var message = cleanField(fields.message, 2000);
  var allowedAnswers = [
    "Có, tôi sẽ đến chung vui",
    "Rất tiếc, tôi chưa thể tham dự"
  ];

  if (!guest || !message || allowedAnswers.indexOf(attending) === -1) {
    return ContentService.createTextOutput("Invalid response");
  }

  var lock = LockService.getScriptLock();
  lock.waitLock(10000);
  try {
    var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
    if (!spreadsheet) {
      throw new Error("Bind this script to the response spreadsheet.");
    }

    var sheet = spreadsheet.getSheetByName("Phản hồi") ||
      spreadsheet.insertSheet("Phản hồi");
    if (sheet.getLastRow() === 0) {
      sheet.appendRow(["Thời gian", "Họ tên", "Tham dự", "Lời chúc"]);
    }
    sheet.appendRow([
      new Date(),
      safeCell(guest),
      attending,
      safeCell(message)
    ]);
  } finally {
    lock.releaseLock();
  }

  return ContentService.createTextOutput("OK");
}

function cleanField(value, maxLength) {
  return String(value || "")
    .replace(/[\r\n\t]/g, " ")
    .trim()
    .slice(0, maxLength);
}

function safeCell(value) {
  return /^[=+\-@]/.test(value) ? "'" + value : value;
}
