import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';

@Component({
  selector: 'app-root',
  imports: [ShellComponent, RouterOutlet],
  template: `<app-shell><router-outlet /></app-shell>`,
})
export class App {}
