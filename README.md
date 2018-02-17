# フレンズ言語 (Friends-lang)

[![Build Status](https://travis-ci.org/vain0/friends-lang.svg?branch=master)](https://travis-ci.org/vain0/friends-lang)

**フレンズ言語** は、ジャパリパークのフレンズのためのプログラミング言語。

- [すごーい！ きみはプログラミング言語を実装できるフレンズなんだね - Qiita](http://qiita.com/vain0/items/6d3b75f667d3ec7f1d2a)

## インストールと使い方

- [最新版のパッケージをダウンロード](https://github.com/vain0/friends-lang/releases/latest)して展開します。
    - Windows の場合は `friendsi-win-x64.zip`
    - macOS の場合は `friendsi-osx-x64.zip`
    - Linux の場合は `friendsi-linux-x64.zip`
- `friendsi.exe` (または `friendsi`) を実行します。
- ようこそジャパリパークへ！

## 文法
### すごーい！ 文
すごーい！ 文では、事実を述べることができる。

```
すごーい！ かばんちゃん は ヒトの フレンズ なんだね！
```

条件つきのすごーい！文では、仮定が真である場合に、結論も真であることを述べる。以下の例では、命題「あなた が ヒトの フレンズ」が真である場合に、命題「あなた は しっぽのない フレンズ なんだね！」も真であることを述べている。

```
すごーい！ あなた が ヒトの フレンズ なら
あなた は しっぽのない フレンズ なんだね！
```

### なんだっけ？ 文
なんだっけ？ 文では、事実を確認したり、一定の性質を満たすものを探索したりできる。

```
すごーい！ かばんちゃん は ヒトの フレンズ なんだね！
すごーい！ あなた が ヒトの フレンズ なら
あなた は しっぽのない フレンズ なんだね！

だれ が しっぽのない フレンズ なんだっけ？
```

出力:

```
「だれ」は「かばんちゃん」、
あってる？ (y/n)Y
やったー！
```

この なんだっけ？ 文では「かばんちゃんはヒトのフレンズ」かつ「ヒトのフレンズはしっぽのないフレンズ」だから「かばんちゃんはしっぽのないフレンズ」である、と推論している。

## 推論
なんだっけ？ 文の実行の手順について解説する。

最初の手順は、与えられた命題の定義を探索することである。例えば、先述の文

```
だれ が しっぽのない フレンズ なんだっけ？
```

を実行するには、まず

```
X が しっぽのない フレンズ なんだね！
```

で終わる すごーい！ 文を探す。そして次が見つかる。

```
すごーい！ あなた が ヒトの フレンズ なら
あなた は しっぽのない フレンズ なんだね！
```

次に、次の2つの命題を**単一化**する。

- ``だれ が しっぽのない フレンズ``
- ``あなた が しっぽのない フレンズ``

単一化とは、2つの項や命題が同一になるように、変数の値を埋めていく処理をいう。この例では、変数「だれ」＝変数「あなた」という割り当てにより、2つの命題を同じ「あなた が しっぽのない フレンズ」に単一化できる。

条件つきのすごーい！文は、仮定が真である場合にのみ、結論も真であると主張している。すなわち、この段階では、まだ命題

```
あなた が しっぽのない フレンズ
```

が真かどうかは分からない。そのため、仮定

```
あなた が ヒトの フレンズ
```

の真偽を判定する必要がある。繰り返しになるが、「X が ヒトの フレンズ」で終わる すごーい！ 文を探索して、

```
すごーい！ かばんちゃん は ヒトの フレンズ なんだね！
```

を見つける。「あなた が ヒトの フレンズ」と「かばんちゃん は ヒトの フレンズ」を単一化して、「あなた」＝「かばんちゃん」という割り当てを得る。このすごーい！ 文は無条件に成り立つので、確認すべき仮定はない。

結局、変数「だれ」＝変数「あなた」＝「かばんちゃん」という割り当てにおいて、命題

```
だれ が しっぽのない フレンズ
```

すなわち

```
かばんちゃん が しっぽのない フレンズ
```

が真であることが分かる。

Friends 言語の仕組みは、「単一化」と「命題の真偽の判定」だけである。

## 項
Friends 言語の **項** について説明する。(他の言語では式と呼ぶことが多いが、ここでは伝統に従って項と呼ぶ。)

### 変数とアトム
「だれ」「あなた」「きみ」などのいくつかの単語と、アンダーバー _ で始まる単語は、**変数** という。単一化において、変数は任意の項と単一化できる。ただし、既に変数 X が項 t に単一化されている状況では、同じ変数 X は t にのみマッチする。例えば、項「X と X」は「サーバル と サーバル」や「かばんちゃん と かばんちゃん」にマッチするが、「サーバル と かばんちゃん」にはマッチしない。

それ以外の単語 (先ほどの「かばんちゃん」など) は **アトム** という。単に文字列のことだと思ってかまわない。

### 複合項
``t の a`` という形の項を **複合項** という。複合項は、同じ形の複合項とマッチする。

### リスト
先に詳しい人のためにいっておくと、Friends のリストは cons セルであり、nil は単なるアトムである。

``t_1 と t_2 と … と t_n`` という形の項を **リスト** という。リストは、同じ形のリストとマッチするが、後述の尾部を持つリストともマッチする。

``t_1 と t_2 と … と t_n と ts とか`` という形の項を `ts` を **尾部とするリスト** という。これは平たくいえば、リスト ``t_1 と t_2 と … と t_n`` にリスト `ts` を連結したものだと思ってかまわない。例えば、変数 `_Xs` を尾部とするリスト ``a と b と _Xs とか`` は次のように単一化される。

| 対象 | `_Xs` の値 |
|:--:|:--:|
| ``a と b`` | 長さ 0 のリスト |
| ``a と b と c`` | `c` を含む長さ 1 のリスト |
| ``a と b と c と d`` | ``c と d`` |

`と` は `の` より優先順位が低い。「リスト の a」という形の複合項を記述する場合は、括弧 「」 を用いて、 ``「x と y」の a`` と書く。

### 自然数
自然数 (0, 1, 2, ...) は、アトムや複合項の略記である。まず、`0` は単にアトム `0` である。n = m + 1 とするとき、項 n は ``m の 次`` という複合項を表す。要するに、次の表のようになる。

| 自然数 | 項 |
|:--:|:--:|
| 0 | `0` |
| 1 | ``0 の 次`` |
| 2 | ``0 の 次 の 次`` |
| 3 | ``0 の 次 の 次 の 次`` |

したがって、

```
すごーい！ 3 は three フレンズ なんだね！
あなた の 次 は three フレンズ なんだっけ？
```

とすると、変数「あなた」 = ``0 の 次 の 次`` = `2` という割り当てが得られる。
