all:
	./remove_post_build.py
	xbuild /p:Configuration=Release\;Platform=x64 Mono.sln
debug:
	xbuild /p:Configuration=Debug\;Platform=x64 Mono.sln
clean:
	xbuild /t:Clean Mono.sln
