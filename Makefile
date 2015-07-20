# Makefile for building SolarSailNavigator

KSPDIR	:= ${HOME}/.local/share/Steam/steamapps/common/Kerbal Space Program
MANAGED	:= ${KSPDIR}/KSP_Data/Managed/
PT	:= ${KSPDIR}/GameData/PersistentThrust/Plugins/
MM	:= ${KSPDIR}/GameData/
SSFILES	:= src/SolarSailControlled.cs \
	src/SailControls.cs \
	src/Preview.cs \
	src/Frames.cs
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
	@echo "  LTP:     ${LTP}"
	@echo "================================"

build: build/SolarSailNavigator.dll

build/%.dll: ${SSFILES}
	mkdir -p build
	${GMCS} -t:library -lib:"${MANAGED}" -lib:"${PT}" -lib:"${MM}" \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine,PersistentThrust \
		-out:$@ \
		${SSFILES}

package: build ${SSFILES}
	mkdir -p package/SolarSailNavigator/Plugins
	cp build/SolarSailNavigator.dll package/SolarSailNavigator/Plugins/
	cp -r Patches package/
	cp License.md README.org TODO.org CHANGELOG.org package/SolarSailNavigator/

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
	cp build/SolarSailNavigator.dll "${KSPDIR}"/GameData/SolarSailNavigator/Plugins/
	cp -r Patches "${KSPDIR}"/GameData/SolarSailNavigator/

uninstall: info
	rm -rf "${KSPDIR"/GameData/SolarSailNavigator/Plugins

.PHONY : all info build package tar.gz zip clean install uninstall
