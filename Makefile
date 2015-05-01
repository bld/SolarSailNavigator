# Makefile for building SolarSailNavigator

KSPDIR	:= ${HOME}/.local/share/Steam/steamapps/common/Kerbal Space Program
MANAGED	:= ${KSPDIR}/KSP_Data/Managed/
SSFILES	:= src/Controls.cs \
	src/Preview.cs \
	src/SolarSailNavigator.cs
GMCS	:= gmcs
TAR	:= tar
ZIP	:= zip

all: build

info:
	@echo "== SolarSailNavigator Build Information =="
	@echo "  gmcs:    ${GMCS}"
	@echo "  tar:     ${TAR}"
	@echo "  zip:     ${ZIP}"
	@echo "  KSP Data: ${KSPDIR}"
	@echo "================================"

build: build/SolarSailNavigator.dll

build/%.dll: ${SSFILES}
	mkdir -p build
	${GMCS} -t:library -lib:"${MANAGED}" \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-out:$@ \
		${SSFILES}

package: build ${SSFILES}
	mkdir -p package/SolarSailNavigator/Plugins
	cp -r Parts package/SolarSailNavigator/
	cp build/SolarSailNavigator.dll package/SolarSailNavigator/Plugins/
	cp LICENSE.md README.md package/SolarSailNavigator/

%.tgz:
	cd package; ${TAR} zcf ../$@ SolarSailNavigator

tgz: package SolarSailNavigator.tgz

%.zip:
	cd package; ${ZIP} -9 -r ../$@ SolarSailNavigator

zip: package SolarSailNavigator.zip

clean:
	@echo "Cleaning up build and package directories..."
	rm -rf build/ package/

install: build
	mkdir -p "${KSPDIR}"/GameData/SolarSailNavigator/Plugins
	cp -r Parts "${KSPDIR}"/GameData/SolarSailNavigator/
	cp build/SolarSailNavigator.dll "${KSPDIR}"/GameData/SolarSailNavigator/Plugins/

uninstall: info
	rm -rf "${KSPDIR"/GameData/SolarSailNavigator/Plugins
	rm -rf "${KSPDIR"/GameData/SolarSailNavigator/Parts

.PHONY : all info build package tar.gz zip clean install uninstall
