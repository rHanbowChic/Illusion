from PIL import Image
import sys
arg1=int(sys.argv[1]);arg2=int(sys.argv[2]);arg3=int(sys.argv[3])
im=Image.open("blank.png")
im.convert('RGBA')
imgblender=Image.new('RGBA',im.size,(arg1,arg2,arg3))
m,n=im.size
im.paste(imgblender,(0,0,m,n),imgblender)
icon=Image.open("exeicon.png")
p=Image.new('RGBA',icon.size,(arg1,arg2,arg3))
x,y=icon.size
p.paste(icon,(0,0,x,y),icon)
icon=p
im.paste(icon,(59,59))
im.save("tile.png")

im=Image.open("blankSmall.png")
im.convert('RGBA')
imgblender=Image.new('RGBA',im.size,(arg1,arg2,arg3))
m,n=im.size
im.paste(imgblender,(0,0,m,n),imgblender)
icon=Image.open("exeicon.png")
p=Image.new('RGBA',icon.size,(arg1,arg2,arg3))
x,y=icon.size
p.paste(icon,(0,0,x,y),icon)
icon=p
im.paste(icon,(9,9))
im.save("tileSmall.png")