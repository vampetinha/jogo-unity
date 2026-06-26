from PIL import Image
import glob, os

PASTA = r"D:\Jogo_Unity\jogo-unity\Assets\mapa"
LIMIAR = 8  # só remove preto puro ou quase puro (não afeta cinza escuro do chão)

arquivos = glob.glob(os.path.join(PASTA, "*.png"))
print(f"Encontrados {len(arquivos)} PNGs...")

for path in arquivos:
    img = Image.open(path).convert("RGBA")
    dados = img.getdata()
    novos = []
    for r, g, b, a in dados:
        if r < LIMIAR and g < LIMIAR and b < LIMIAR:
            novos.append((0, 0, 0, 0))  # transparente
        else:
            novos.append((r, g, b, a))
    img.putdata(novos)
    img.save(path)
    print(f"  OK: {os.path.basename(path)}")

print("\nPronto! Reimporte os sprites no Unity (clique com botao direito na pasta -> Reimport All).")
