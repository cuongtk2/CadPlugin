# `.PAT` File Format

**Written by Hùng Mạnh Cường — 23/10/2026**

**Language / Ngôn ngữ:** [🇻🇳 Tiếng Việt](#tiếng-việt) · [🇬🇧 English](#english)

---

<a id="tiếng-việt"></a>

## 🇻🇳 Tiếng Việt

### Định dạng file `.PAT`

```text
*TÊN, mô tả
a, b,c, d,e
a, b,c, d,e, f,g,...
```

- `a, b,c, d,e`: Họ đường liền nét.
- `a, b,c, d,e, f,g,...`: Họ đường có kiểu nét.
- Mỗi dòng dữ liệu mô tả một họ đường song song kéo dài vô hạn.

Các giá trị cách nhau bằng dấu phẩy; số thập phân dùng dấu chấm. Dấu `;` bắt đầu phần chú thích. File phải có một dòng trống ở cuối.

### Các tham số

#### `a` — Góc đường (độ)

Hướng của đường thẳng so với trục X dương, tính ngược chiều kim đồng hồ. `0°` = nằm ngang, `90°` = thẳng đứng, `45°` = chéo lên phải. Khi HATCH, góc Angle của lệnh được cộng thêm vào `a`.

#### `b, c` — Điểm gốc (x, y)

Điểm mà đường đầu tiên đi qua, đồng thời là chỗ nét đầu tiên (`f`) bắt đầu. Dùng để canh vị trí pattern so với gốc tọa độ `(0,0)`.

#### `d` — Delta-X (dịch dọc theo đường)

Khi sang đường kế tiếp, điểm gốc bị dời `d` đơn vị **dọc theo hướng của đường**. Dùng để tạo kiểu so le, chẳng hạn gạch xếp lệch. Nếu đường liền nét, không có dash, thì `d` không làm thay đổi hình.

#### `e` — Delta-Y (khoảng cách giữa các đường)

Khoảng cách **vuông góc** giữa hai đường kề nhau, đo về phía bên trái hướng đường (`a + 90°`).

- `e > 0`: Các đường nối tiếp mọc về phía đó.
- `e < 0`: Các đường mọc về phía ngược lại.
- `e = 0`: Preview bỏ qua dòng này.

**Lưu ý:** `d` và `e` nằm trong hệ tọa độ của chính đường (đã xoay theo `a`), không phải tọa độ X/Y của bản vẽ. Ví dụ `a = 45°` thì `d` chạy dọc theo phương `45°`, còn `e` chạy theo phương `135°`.

Công thức: đường thứ `k` đi qua điểm

\[
P_k = (b,c) + k(d\,u + e\,v)
\]

Trong đó:

\[
u=(\cos a,\sin a),\qquad v=(-\sin a,\cos a)
\]

`u` là vector hướng đường; `v` là vector vuông góc hướng về bên trái.

#### `f` — Độ dài nét liền

Độ dài đoạn được vẽ.

#### `g` — Độ dài khoảng hở

Khoảng hở đến nét tiếp theo, biểu diễn bằng số âm.

Có thể khai báo nhiều giá trị `f,g,...` liên tiếp. Ví dụ:

```text
10, -5, 2, -3
```

Tương ứng: nét dài `10`, hở `5`, nét dài `2`, hở `3`, rồi lặp lại.

### Ví dụ

```text
*GACH, Gach 200x100 xep so le
0, 0,0, 0,100
90, 0,0, 100,100, 100,-100
```

- **Dòng 1:** Đường ngang qua `(0,0)`, cách nhau `100` theo phương đứng, liền nét.
- **Dòng 2:** Đường đứng qua `(0,0)`, cách nhau `100` theo phương ngang (`e`). Mỗi đường kế tiếp dời lên `100` dọc theo đường (`d`), tạo kiểu so le; nét `100`, hở `100`.

[⬆ Về đầu trang](#pat-file-format)

---

<a id="english"></a>

## 🇬🇧 English

### `.PAT` file format

```text
*NAME, description
a, b,c, d,e
a, b,c, d,e, f,g,...
```

- `a, b,c, d,e`: A solid line family.
- `a, b,c, d,e, f,g,...`: A dashed line family.
- Each data line describes a family of parallel lines extending to infinity.

Values are separated by commas; use a period as the decimal point. A semicolon (`;`) starts a comment. The file must end with a blank line.

### Parameters

#### `a` — Line angle (degrees)

Direction of the line measured counter-clockwise from the positive X axis. `0°` = horizontal, `90°` = vertical, `45°` = diagonal up to the right. When hatching, the command's Angle is added to `a`.

#### `b, c` — Origin point (x, y)

The point the first line passes through, and where the first dash (`f`) starts. Use it to position the pattern relative to the drawing origin `(0,0)`.

#### `d` — Delta-X (shift along the line)

When moving to the next line, the origin shifts by `d` units **along the line direction**. This is used for staggered patterns, such as offset bricks. For a solid line with no dashes, `d` does not change the pattern.

#### `e` — Delta-Y (spacing between lines)

The **perpendicular** distance between adjacent lines, measured to the left of the line direction (`a + 90°`).

- `e > 0`: Successive lines extend toward that side.
- `e < 0`: Successive lines extend toward the opposite side.
- `e = 0`: The preview skips this line.

**Note:** `d` and `e` use the line's own coordinate system, rotated by `a`; they are not the drawing's X/Y coordinates. For example, when `a = 45°`, `d` runs along `45°`, while `e` runs along `135°`.

Formula: line `k` passes through

\[
P_k = (b,c) + k(d\,u + e\,v)
\]

where

\[
u=(\cos a,\sin a),\qquad v=(-\sin a,\cos a)
\]

`u` is the line-direction vector; `v` is the perpendicular vector pointing to the left.

#### `f` — Dash length

The length of the drawn segment.

#### `g` — Gap length

The gap before the next dash, represented by a negative number.

Multiple `f,g,...` values can be specified in sequence. For example:

```text
10, -5, 2, -3
```

This means: dash `10`, gap `5`, dash `2`, gap `3`, then repeat.

### Example

```text
*GACH, Gach 200x100 xep so le
0, 0,0, 0,100
90, 0,0, 100,100, 100,-100
```

- **Line 1:** Horizontal lines through `(0,0)`, spaced `100` apart vertically, solid.
- **Line 2:** Vertical lines through `(0,0)`, spaced `100` apart horizontally (`e`). Each successive line shifts upward by `100` along the line (`d`), creating the stagger; dash `100`, gap `100`.

[⬆ Back to top](#pat-file-format)

