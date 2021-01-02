import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'cast'
})
export class CastPipe implements PipeTransform {

  transform(value: unknown): any {
    return value;
  }

}
