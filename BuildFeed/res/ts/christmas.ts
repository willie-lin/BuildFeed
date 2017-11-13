"use script";

module BuildFeed.Christmas
{
    const movementSpeed = 5;

    let snowContainer: HTMLDivElement;
    let snow: Array<Snowflake> = [];
    let counter: number = 0;
    let tick: number = 0;

    class Snowflake
    {
        private readonly element: HTMLDivElement;
        private top: number;
        private left: number;

        public getElement()
        {
            return this.element;
        }

        public bumpElement(moveX: boolean)
        {
            let localSpeed = Math.round(((Math.random() / 2) + 0.5) * movementSpeed);
            this.top = this.top + localSpeed;
            if (this.top < -10 || this.top > (window.innerHeight + 10))
            {
                this.element.remove();
                return true;
            }

            if (moveX)
            {
                localSpeed = Math.round(((Math.random() / 2) + 0.5) * movementSpeed);
                this.left = Math.random() > 0.5
                    ? this.left + localSpeed
                    : this.left - localSpeed;

                if (this.left < -10 || this.left > (window.innerWidth + 10))
                {
                    this.element.remove();
                    return true;
                }
            }

            this.element.style.transform = `translate(${this.left}px, ${this.top}px)`;
            return false;
        }

        constructor()
        {
            this.element = document.createElement("div");
            this.top = 0;
            this.left = Math.round(Math.random() * window.innerWidth);
        }
    }

    function deferAnimate()
    {
        window.requestAnimationFrame(animate);
    }

    function animate()
    {
        if (counter % 5 === 0)
        {
            for (let i = 0; i < 2; i++)
            {
                const newSnow = new Snowflake();
                snow.push(newSnow);
                snowContainer.appendChild(newSnow.getElement());
            }
        }

        for (const flake of snow)
        {
            const result = flake.bumpElement(true);

            if (result)
            {
                snow.splice(snow.indexOf(flake), 1);
            }
        }

        counter++;
    }

    export function Setup()
    {
        snowContainer = document.createElement("div");
        snowContainer.className = "snow-container";
        document.body.appendChild(snowContainer);

        window.requestAnimationFrame(animate);
        setInterval(deferAnimate, 125);
    }
}

window.addEventListener("load", BuildFeed.Christmas.Setup);