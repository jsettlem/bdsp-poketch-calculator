using System.Diagnostics;
using System.Globalization;

namespace BDSPCalculator; 

public class PoketchCalculator {
    public CultureInfo locale = CultureInfo.CurrentCulture;
    public const string defaultDisp = "0";
    public const int maxDigit = 10;
    
    public Sprite[] numImage = {
        new(), new(), new(), new(),
        new(), new(), new(), new(),
        new(), new()
    };

    public string dispNumString = "";
    public Sprite symbol  = new();
    public Decimal currentNum;
    
    public decimal parsedNumber;
    public string integerPart;
    public string decimalPart;
    public CalcCode calcMode;
    public bool isNegative;
    public int dotIndex;

    public enum CalcCode
    {
       Num_0, //0x0
       Num_1,
       Num_2,
       Num_3,
       Num_4,
       Num_5,
       Num_6,
       Num_7,
       Num_8,
       Num_9, //0x9
       Num_Point, //0xa
       Act_Plus, //0xb
       Act_Minus, //0xc
       Act_Mul, //0xd
       Act_Div, //0xe
       Act_Equrl, //0xf
       Act_Clear, //0x10
       Symbol_Asterisk, //0x11
       Symbol_Question, //0x12
       CalcCode_Null, //ox13
    }

    public void InputClear() {
        currentNum = Decimal.Zero;
        calcMode = CalcCode.CalcCode_Null;
        dispNumString = "0";

        UpdateNumberImage();

        symbol.enabled = true;
        symbol.sprite = CalcCode.CalcCode_Null;
    }

    public void InputNumeric(CalcCode num) {
        if (calcMode == CalcCode.Act_Equrl) {
            currentNum = Decimal.Zero;
            calcMode = CalcCode.CalcCode_Null;
            dispNumString = "0";
        }

        if (dispNumString.Equals("0")) {
            dispNumString = String.Empty;
            
            foreach (var n in numImage) {
                n.enabled = false;
            }
        }

        if (dispNumString.Length < 10) {
            var newNum_str = String.Format("{0}", (int) num);
            dispNumString = String.Concat(dispNumString, newNum_str);
        }
        
        UpdateNumberImage();

        symbol.enabled = false;
        if (calcMode != CalcCode.CalcCode_Null && calcMode != CalcCode.Act_Clear) {
            symbol.enabled = true;
            symbol.sprite = calcMode;
        }
    }

    public void InputDigitPoint() {
        if (calcMode != CalcCode.CalcCode_Null) {
            if (calcMode == CalcCode.Act_Equrl) {
                currentNum = decimal.Zero;
                calcMode = CalcCode.CalcCode_Null;
                dispNumString = "0";
            }

            if (dispNumString == "0") {
                foreach (var n in numImage) {
                    n.enabled = false;
                }
            }
        }

        if (dispNumString.Length < 8) {
            dispNumString = String.Concat(dispNumString, ".");
        }
        
        UpdateNumberImage();
    }

    public void InputAction(CalcCode action) {
        if (currentNum == decimal.Zero && calcMode == CalcCode.Act_Minus) {
            if (dispNumString != "0") {
                dispNumString = String.Concat("-", dispNumString);
                calcMode = CalcCode.Act_Equrl;
                UpdateNumberImage();
                goto joined;
            }
        }

        calcMode = action;

        if (currentNum == Decimal.Zero) {
            decimal newDecimal;
            if (decimal.TryParse(dispNumString, NumberStyles.Number, locale, out newDecimal)) {
                currentNum = newDecimal;
                dispNumString = "0";
            }
        }

        joined:
        symbol.enabled = false;
        if (calcMode != CalcCode.CalcCode_Null && calcMode != CalcCode.Act_Clear) {
            symbol.enabled = true;
            symbol.sprite = action;
        }
    }

    public void InputEqual() {
        decimal parsedNum;
        var result = Decimal.TryParse(dispNumString, NumberStyles.Number, locale, out parsedNum);

        if (!result) {
            return;
        }

        switch (calcMode) {
            case CalcCode.Act_Plus:
                currentNum += parsedNum;
                break;
            case CalcCode.Act_Minus:
                currentNum -= parsedNum;
                break;
            case CalcCode.Act_Mul:
                currentNum *= parsedNum;
                break;
            case CalcCode.Act_Div:
                if (parsedNum == 0) break;

                currentNum /= parsedNum;
                break;
        }

        dispNumString = currentNum.ToString(locale);
        
        UpdateNumberImage();

        symbol.enabled = false;
        if (calcMode != CalcCode.CalcCode_Null && calcMode != CalcCode.Act_Clear) {
            symbol.enabled = true;
            symbol.sprite = calcMode;
        }

        if (dispNumString.Contains(".")) {
            if (dispNumString.Length > 10) {
                dispNumString = dispNumString[..10];
                var i = dispNumString.Length;
                while (i > 1 && dispNumString.EndsWith("0")) {
                    dispNumString = dispNumString[..^1];
                }
            }
        }

        calcMode = CalcCode.Act_Equrl;
        currentNum = Decimal.Zero;
    }

    private void UpdateNumberImage() {
        if (!string.IsNullOrEmpty(dispNumString)) {
            var success = decimal.TryParse(dispNumString, NumberStyles.Number, locale, out parsedNumber);
            if (!success) return; 

            isNegative = false;
            if (parsedNumber < decimal.Zero) {
                isNegative = true;
                parsedNumber = -parsedNumber;
                dispNumString = dispNumString.Replace("-", "");
            }

            dotIndex = dispNumString.IndexOf(".");
            var integerLength = 0;

            integerPart = dispNumString;
            decimalPart = "";
            
            if (dotIndex > 0) {
                integerPart = integerPart.Substring(0, dotIndex);
                integerLength = integerPart.Length;
                if (integerLength < 8) { 
                    var decimalLength = dispNumString.Length;
                    
                    if (decimalLength > 9) {
                        decimalLength = 10;
                    }

                    decimalLength += ~integerLength;

                    if (decimalLength > 7) {
                        decimalLength = 8;
                    }

                    if (decimalLength > 0) {
                        decimalPart = dispNumString.Substring(dotIndex + 1, decimalLength);
                    }
                }
            }

            if (integerPart.Length > 10) return;
            
            foreach (var n in numImage) {
                n.enabled = false;
            }

            if (decimalPart.Length > 0) {
                UInt64 parsedDecimal;
                
                success = UInt64.TryParse(decimalPart,NumberStyles.Integer, locale, out parsedDecimal);
                if (!success) return;

                SetNumImage(parsedDecimal, decimalPart.Length,  0);
            }

            var integerOffset = 0;

            if (decimalPart.Length != 0 || dotIndex > -1) {
                integerOffset = decimalPart.Length + 1;
            }
            
            SetNumImage((ulong)parsedNumber, integerPart.Length, integerOffset);

            if (dotIndex > 0) {
                if (numImage.Length > integerOffset - 1) {
                    numImage[integerOffset - 1].enabled = true;
                }

                numImage[integerOffset - 1].sprite = CalcCode.Num_Point;
            }

            if (isNegative) {
                if (numImage.Length > dispNumString.Length) {
                    numImage[dispNumString.Length].enabled = true;
                    numImage[dispNumString.Length].sprite = CalcCode.Act_Minus;
                }
            }
        }
    }

    private void SetNumImage(UInt64 num, int digit, int dispIndex) {
        if (digit <= 0) return;

        var place = 0;
        do {
            UInt64 new_number = 0;
            UInt64 powTen = (ulong)Math.Pow(10, place);
            
            var placeIndex = dispIndex + place;
            var next_pow_ten = powTen * 10;
            
            if (next_pow_ten != 0) {
                new_number = (num / next_pow_ten);
            }

            UInt64 remainder = 0;
            if (powTen != 0) {
                remainder = (num - new_number * next_pow_ten) / powTen;
            }

            numImage[placeIndex].enabled = true;
            numImage[placeIndex].sprite = (CalcCode)remainder;

            place += 1;
        } while (place != digit);
    }

    public PoketchCalculator() {
        InputClear();
    }
}

public class Sprite {
    public bool enabled;
    public PoketchCalculator.CalcCode sprite = PoketchCalculator.CalcCode.CalcCode_Null;
}
