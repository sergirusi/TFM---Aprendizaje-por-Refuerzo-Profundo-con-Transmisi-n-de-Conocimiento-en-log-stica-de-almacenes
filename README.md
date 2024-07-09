# Trabajo Final de Máster

Este es el Trabajo Final de Máster (TFM) del Máster en Inteligencia Artificial en la Universidad Internacional de la Rioja (UNIR).

## Resumen

Este trabajo investiga la aplicación del Aprendizaje por Refuerzo Profundo (DRL) y la Transferencia de Conocimiento (KT) en la logística de almacenes, utilizando Unity y ML-Agents. Los resultados muestran que KT mejora el rendimiento y la eficiencia del aprendizaje, permitiendo a los agentes completar tareas de manera más rápida y estable.

El trabajo futuro incluye la integración de sensores avanzados, optimización de hiperparámetros, validación en entornos reales, desarrollo de políticas de aprendizaje continuo, colaboración entre múltiples agentes y la inclusión de factores económicos y de eficiencia energética.

## Instalación y configuración del entorno de trabajo

### Herramientas Necesarias

- **Python 3.9.13**: [Descargar Python](https://www.python.org/downloads/release/python-3913/)
- **Unity Hub**: [Descargar Unity Hub](https://unity.com/es/download)
- **Visual Studio Code**: [Descargar Visual Studio Code](https://code.visualstudio.com/download)
- **Cmder (opcional)**: [Descargar Cmder](https://cmder.app/)

### Instalación de Unity Warehouse Assets

- **Unity Warehouse Assets**: [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/industrial/unity-warehouse-276394)

1. Crear un nuevo proyecto en Unity.
2. Descargar e importar el asset *UnityWarehouseSceneHDRP*.
3. Configurar el renderizado HDRP en *Edit -> Project Settings -> Graphics*.

### Configuración del entorno de trabajo

Para la configuración del entorno de trabajo, se creó un nuevo proyecto en Unity en el que se desarrolló la primera prueba DRL con MLAgents. Tras crear el proyecto, se procedió a crear el entorno virtual de trabajo Python. A continuación, se mostrarán los pasos realizados para su correcta configuración:

1. Acceder a un terminal.
2. Acceder al directorio del proyecto Unity con el comando `cd ruta/del/proyecto`.
3. Creación del entorno virtual con el comando `python -m venv [Nombre del entorno]`.
4. Acceso al entorno virtual con el comando `[Nombre del entorno]/Scripts/activate`.
5. Actualización de pip con el comando `python -m pip install --upgrade pip`.
6. Instalación de las dependencias necesarias para utilizar ML-Agents:
    - `pip3 install mlagents`
    - `pip3 install torch torchvision torchaudio`
    - `pip install protobuf==3.20.3`
    - `pip install onnx`
7. Acceder a un terminal Cmder (Opcional: los siguientes pasos son para la automatización del levantamiento del entorno desde el terminal Cmder).
8. Seleccionar el desplegable del icono `+` y seleccionar la opción *"Setup tasks..."*.
9. Clonar una *Task* con el botón inferior *"Clone"*.
10. Renombrar la nueva *Task*.
11. Dentro del campo de texto de comandos, añadir el siguiente comando:
    ```cmd
    cmd /k "%ConEmuDir%\..\init.bat" "  -new_console:d:\ruta\del\proyecto\[nombre del entorno]\Scripts" && activate && cd ruta\del\proyecto\
    ```
    Este comando permitirá activar el entorno virtual Python creado y acceder al directorio base del proyecto para poder trabajar con el mismo.
12. Guardar los cambios con el botón *"Save settings"*.
13. Seleccionar la nueva *Task* desde el menú desplegable del icono `+` y el entorno está listo para ser usado.

## Contenido del Proyecto

- **Assets**: Recursos del proyecto (modelos 3D, texturas, scripts, etc.).
- **Packages**, **ProjectSettings**: Directorios de configuración de Unity.
- **.gitignore**: Archivo de configuración de Git.
- **LICENSE**: Licencia del proyecto.
- **README.md**: Archivo de descripción del proyecto.

## Uso

1. Abrir la escena principal en Unity desde la carpeta `Assets\Scenes\TFM.unity`.
2. Ejecutar el comando de ML-Agents en el terminal para entrenar el modelo: `mlagents-learn <nombre del yaml de config> --run-id=<nombre del entrenamiento>` o `mlagents-learn <nombre del yaml de config> --initialize-from=<nombre del modelo preentrenado para iniciar KT> --run-id=<nombre del entrenamiento>`.
3. Ejecutar el proyecto haciendo clic en `Play` en Unity para inicializar el entrenamiento.


## Licencia

Este proyecto está bajo la licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.
