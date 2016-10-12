# Makefile for building SolarSailNavigator

KSPDIR	:= ${HOME}/.local/share/Steam/steamapps/common/Kerbal Space Program
MANAGED	:= ${KSPDIR}/KSP_Data/Managed/
PT	:= ${KSPDIR}/GameData/PersistentThrust/Plugins/
MM	:= ${KSPDIR}/GameData/
SSFILES	:= src/Default.cs \
	src/Utils.cs \
	src/Navigator.cs \
	src/Controls.cs \
	src/Preview.cs \
	src/Frames.cs
GMCS	:= gmcs
TAR	:= tar
ZIP	:= zip

all: 	build

info:
	@echo "== SolarSailNavigator Build Information =="
	@echo "  gmcs:     ${GMCS}"
	@echo "  tar:      ${TAR}"
	@echo "  zip:      ${ZIP}"
	@echo "  KSP Data: ${KSPDIR}"
	@echo "  PT:       ${PT}"
	@echo "================================"

build: build/SolarSailNavigator.dll

build/%.dll: ${SSFILES}
	mkdir -p build
	${GMCS} -t:library -lib:"${MANAGED}" -lib:"${PT}" -lib:"${MM}" \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine,PersistentThrust,UnityEngine.UI \
		-out:$@ \
		${SSFILES}

package: build ${SSFILES}
	mkdir -p package/SolarSailNavigator/Plugins
	cp build/SolarSailNavigator.dll package/SolarSailNavigator/Plugins/
	cp LICENSE.txt README.org TODO.org CHANGELOG.org ISSUES.org package/SolarSailNavigator/
	cp -r Patches package/SolarSailNavigator/

%.tgz:
	cd package; ${TAR} zcf ../$@ GameData

tgz: package SolarSailNavigator.tgz

%.zip:
	cd package; ${ZIP} -9 -r ../$@ SolarSailNavigator

zip: package SolarSailNavigator.zip

clean:
	@echo "Cleaning up build and package directories..."
	rm -rf build/ package/

install: package
	cp -r package/SolarSailNavigator "${KSPDIR}"/GameData/

uninstall: info
	rm -rf "${KSPDIR}"/GameData/SolarSailNavigator/

.PHONY : all info build package tar.gz zip clean install uninstall
